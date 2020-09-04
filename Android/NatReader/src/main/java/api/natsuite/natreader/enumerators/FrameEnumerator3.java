package api.natsuite.natreader.enumerators;

import android.graphics.PixelFormat;
import android.graphics.SurfaceTexture;
import android.media.Image;
import android.media.ImageReader;
import android.media.MediaCodec;
import android.media.MediaExtractor;
import android.media.MediaFormat;
import android.opengl.Matrix;
import android.os.Build;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;
import android.view.Surface;
import java.nio.ByteBuffer;
import java.util.concurrent.ArrayBlockingQueue;
import api.natsuite.natreader.MediaEnumerator;
import api.natsuite.natrender.GLBlitEncoder;
import api.natsuite.natrender.GLRenderContext;

public final class FrameEnumerator3 implements MediaEnumerator {

    private final MediaExtractor extractor;
    private final ArrayBlockingQueue<Frame> framePool;
    private final long endTimestamp;
    private final int frameSkip;
    private static final int POOL_LIMIT = 5;

    private final HandlerThread decoderThread;
    private final HandlerThread imageReaderThread;
    private final ImageReader imageReader;
    private final Handler imageReaderHandler;
    private final GLRenderContext renderContext;
    private final Handler renderContextHandler;

    private int decoderOutputTextureID;
    private SurfaceTexture decoderOutputTexture;
    private Surface decoderOutputSurface;
    private GLBlitEncoder blitEncoder;
    private MediaCodec decoder;

    public FrameEnumerator3 (final MediaExtractor extractor, final MediaFormat format, final float startTime, final float duration, final int frameSkip) {
        // Set time range
        final long startTimeUs = (long)(startTime * 1e+6);
        this.extractor = extractor;
        this.framePool = new ArrayBlockingQueue<>(2 * POOL_LIMIT, false);
        this.endTimestamp = duration > 0 ? startTimeUs + (long)(duration * 1e+6) : Long.MAX_VALUE;
        this.frameSkip = frameSkip;
        extractor.seekTo(startTimeUs, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
        // Don't drop frames
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q)
            format.setInteger("allow-frame-drop", 0);
        // Create image reader
        this.decoderThread = new HandlerThread("NatReader Decoder Thread");
        this.decoderThread.start();
        this.imageReaderThread = new HandlerThread("NatReader Reader Thread");
        this.imageReaderThread.start();
        this.imageReaderHandler = new Handler(imageReaderThread.getLooper());
        this.imageReader = ImageReader.newInstance(format.getInteger(MediaFormat.KEY_WIDTH), format.getInteger(MediaFormat.KEY_HEIGHT), PixelFormat.RGBA_8888, 2);
        this.imageReader.setOnImageAvailableListener(frameHandler, imageReaderHandler);
        // Setup render context
        this.renderContext = new GLRenderContext(null, imageReader.getSurface(), false);
        this.renderContext.start();
        this.renderContextHandler = new Handler(renderContext.getLooper());
        // Create decoder
        renderContextHandler.post(() -> {
            // Create output texture
            decoderOutputTextureID = GLBlitEncoder.getExternalTexture();
            decoderOutputTexture = new SurfaceTexture(decoderOutputTextureID);
            decoderOutputSurface = new Surface(decoderOutputTexture);
            blitEncoder = GLBlitEncoder.externalBlitEncoder();
            // Setup frame delegate
            decoderOutputTexture.setOnFrameAvailableListener(surfaceTexture -> {
                // Update transform
                final float[] transform = new float[16];
                surfaceTexture.updateTexImage();
                surfaceTexture.getTransformMatrix(transform);
                Matrix.translateM(transform, 0, 0.5f, 0.5f, 0.f);
                Matrix.scaleM(transform, 0, 1, -1, 1);
                Matrix.translateM(transform, 0, -0.5f, -0.5f, 0.f);
                // Blit
                blitEncoder.blit(decoderOutputTextureID, transform);
                renderContext.setPresentationTime(surfaceTexture.getTimestamp());
                renderContext.swapBuffers();
            }, renderContextHandler);
            try {
                // Create decoder
                decoder = MediaCodec.createDecoderByType(format.getString(MediaFormat.KEY_MIME));
                decoder.setCallback(decoderCallback, new Handler(decoderThread.getLooper()));
                decoder.configure(format, decoderOutputSurface, null, 0);
                decoder.start();
            }
            catch (Exception ex) {
                Log.e("NatSuite", "NatReader Error: FrameEnumerator failed to create decoder with error: " + ex);
            }
        });
    }

    @Override
    public long copyNextFrame (final ByteBuffer dstBuffer) { // INCOMPLETE // How to know when no frames left?? EOS.
        try {
            final Frame frame = framePool.take();
            GLBlitEncoder.copyFrame(frame.pixelBuffer, frame.width, frame.height, frame.width * 4, dstBuffer); // Should be one block `memcpy`
            return frame.timestamp;
        } catch (InterruptedException ex) {
            Log.e("NatSuite", "NatReader Error: Frame enumerator failed to copy frame", ex);
            return -1L;
        }
    }

    @Override
    public void release () {
        try {
            // Stop decoder handler
            decoderThread.quitSafely();
            // Release rendering resources
            renderContextHandler.post(() -> {
                blitEncoder.release();
                decoderOutputSurface.release();
                decoderOutputTexture.release();
                GLBlitEncoder.releaseTexture(decoderOutputTextureID);
            });
            renderContext.quitSafely();
            //renderContext.join(); // meh, not necessary
            // Release image reader
            imageReader.close();
            imageReaderThread.quitSafely();
            // Stop decoder
            decoder.stop();
            decoder.release();
        } catch (Exception ex) {
            Log.e("NatSuite", "NatReader Error: Frame enumerator encountered error on release", ex);
        }
    }

    private final ImageReader.OnImageAvailableListener frameHandler = new ImageReader.OnImageAvailableListener () {

        @Override
        public void onImageAvailable (ImageReader imageReader) {
            try {
                final Image image = imageReader.acquireLatestImage();
                if (image != null) {
                    final Frame frame = new Frame(image);
                    framePool.put(frame);
                    image.close();
                }
            } catch (Exception ex) {
                Log.e("NatSuite", "NatReader Error: Frame enumerator failed to retrieve video frame", ex);
            }
        }
    };

    private final MediaCodec.Callback decoderCallback = new MediaCodec.Callback () {

        private int frameIndex = 0;

        @Override
        public void onInputBufferAvailable (MediaCodec codec, int bufferIndex) {
            final ByteBuffer inputBuffer = codec.getInputBuffer(bufferIndex);
            final int dataSize = extractor.readSampleData(inputBuffer, 0);
            final long sampleTime = extractor.getSampleTime();
            if (dataSize >= 0 && sampleTime <= endTimestamp)
                codec.queueInputBuffer(bufferIndex, 0, dataSize, sampleTime, extractor.getSampleFlags());
            extractor.advance();
        }

        @Override
        public void onOutputBufferAvailable (MediaCodec codec, int bufferIndex, MediaCodec.BufferInfo bufferInfo) {
            // Check for frame skip
            if (frameIndex++ % (frameSkip + 1) == 0) {
                waitUntil(() -> framePool.size() < POOL_LIMIT); // not a one-to-one guarantee
                codec.releaseOutputBuffer(bufferIndex, true);
            }
            // Discard
            else
                codec.releaseOutputBuffer(bufferIndex, false);
        }

        @Override
        public void onError (MediaCodec codec, MediaCodec.CodecException error) {
            Log.e("NatSuite", "NatReader Error: Frame enumerator decoder encountered error", error);
        }

        @Override
        public void onOutputFormatChanged (MediaCodec codec, MediaFormat mediaFormat) { } // don't care
    };

    interface Predicate {
        boolean get ();
    }

    private static void waitUntil (Predicate condition) {
        try {
            while (!condition.get())
                Thread.sleep(5);
        } catch (InterruptedException ex) { }
    }

    private static final class Frame {

        public final ByteBuffer pixelBuffer;
        public final int width;
        public final int height;
        public final long timestamp;

        public Frame (Image image) {
            Image.Plane imagePlane = image.getPlanes()[0];
            ByteBuffer srcBuffer = imagePlane.getBuffer();
            this.width = image.getWidth();
            this.height = image.getHeight();
            this.timestamp = image.getTimestamp();
            this.pixelBuffer = ByteBuffer.allocateDirect(width * height * 4);
            GLBlitEncoder.copyFrame(srcBuffer, width, height, imagePlane.getRowStride(), pixelBuffer);
            pixelBuffer.rewind();
            pixelBuffer.limit(width * height * 4);
            Log.d("NatSuite", "Frame enumerator created frame for time: "+(timestamp / 1e+9));
        }
    }
}
