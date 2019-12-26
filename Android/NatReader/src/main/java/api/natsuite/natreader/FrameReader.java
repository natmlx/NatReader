package api.natsuite.natreader;

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
import api.natsuite.natrender.GLBlitEncoder;
import api.natsuite.natrender.GLRenderContext;
import api.natsuite.natrender.Unmanaged;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.concurrent.Semaphore;

public final class FrameReader implements MediaReader {

    //region --Client API--

    public FrameReader (String uri, float startTime, float duration) {
        // Setup extractor
        this.extractor = new MediaExtractor();
        try {
            extractor.setDataSource(uri);
        } catch (IOException ex) {
            Log.e("Unity", "NatReader Error: Failed to create media extractor with error: " + ex);
            return;
        }
        // Seek
        final MediaFormat format = videoFormat(extractor);
        long startTimeUs = (long)(startTime * 1e+6);
        startTimeUs = startTimeUs < 0 ? format.getLong(MediaFormat.KEY_DURATION) - startTimeUs : startTimeUs;
        extractor.seekTo(startTimeUs, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
        endTimestamp = duration > 0 ? startTimeUs + (long)(duration * 1e+6) : Long.MAX_VALUE;
        // Inspect
        final int videoWidth = format.getInteger(MediaFormat.KEY_WIDTH);
        final int videoHeight = format.getInteger(MediaFormat.KEY_HEIGHT);
        this.frameRate = Build.VERSION.SDK_INT >= Build.VERSION_CODES.M ? format.getInteger(MediaFormat.KEY_FRAME_RATE) : 30; // Default to 30
        // Setup image reader
        this.imageReaderThread.start();
        this.imageReaderHandler = new Handler(imageReaderThread.getLooper());
        this.imageReader = ImageReader.newInstance(videoWidth, videoHeight, PixelFormat.RGBA_8888, 3);
        // Setup render context
        this.renderContext = new GLRenderContext(null, imageReader.getSurface(), false);
        this.renderContext.start();
        this.renderContextHandler = new Handler(renderContext.getLooper());
        // Create decoder
        final Semaphore completionToken = new Semaphore(0);
        renderContextHandler.post(new Runnable() {
            @Override
            public void run () {
                // Create output texture
                decoderOutputTextureID = GLBlitEncoder.getExternalTexture();
                decoderOutputTexture = new SurfaceTexture(decoderOutputTextureID);
                decoderOutputSurface = new Surface(decoderOutputTexture);
                blitEncoder = GLBlitEncoder.externalBlitEncoder();
                // Create decoder
                try {
                    decoder = MediaCodec.createDecoderByType(format.getString(MediaFormat.KEY_MIME));
                    decoder.configure(format, decoderOutputSurface, null, 0);
                    decoder.start();
                } catch (IOException ex) {
                    Log.e("Unity", "NatReader Error: Failed to start decoder with error: " +  ex);
                } finally {
                    completionToken.release();
                }
            }
        });
        try { completionToken.acquire(); } catch (InterruptedException ex) {}
        Log.d("Unity", "NatReader: Created FrameReader for media at '"+uri+"' with size: "+pixelWidth()+"x"+pixelHeight());
    }

    @Override
    public void release () {
        extractor.release();
        decoder.stop();
        decoder.release();
        renderContextHandler.post(new Runnable() {
            @Override
            public void run () {
                blitEncoder.release();
                decoderOutputSurface.release();
                decoderOutputTexture.release();
                GLBlitEncoder.releaseTexture(decoderOutputTextureID);
            }
        });
        renderContext.quitSafely();
        try { renderContext.join(); } catch (InterruptedException ex) {}
        imageReader.close();
        imageReaderThread.quitSafely();
    }

    @Override
    public long copyNextFrame (final ByteBuffer dstBuffer) {
        // Set callbacks
        final Semaphore renderCompletionToken = new Semaphore(0);
        final Semaphore readbackCompletionToken = new Semaphore(0);
        final class Timestamp { long value = -1L; }
        final Timestamp timestamp = new Timestamp();
        decoderOutputTexture.setOnFrameAvailableListener(new SurfaceTexture.OnFrameAvailableListener() {
            @Override
            public void onFrameAvailable (SurfaceTexture surfaceTexture) {
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
                // Signal completion
                renderCompletionToken.release();
            }
        }, renderContextHandler);
        imageReader.setOnImageAvailableListener(new ImageReader.OnImageAvailableListener() {
            @Override
            public void onImageAvailable (ImageReader imageReader) {
                // Create contiguous buffer with no padding
                final Image image = imageReader.acquireLatestImage();
                if (image != null) {
                    final Image.Plane imagePlane = image.getPlanes()[0];
                    final ByteBuffer sourceBuffer = imagePlane.getBuffer();
                    final int width = image.getWidth();
                    final int height = image.getHeight();
                    final int stride = imagePlane.getRowStride();
                    Unmanaged.copyFrame(Unmanaged.baseAddress(sourceBuffer), width, height, stride, Unmanaged.baseAddress(dstBuffer));
                    dstBuffer.rewind();
                    dstBuffer.limit(width * height * 4);
                    timestamp.value = image.getTimestamp();
                    image.close();
                }
                // Send to waiter
                readbackCompletionToken.release();
            }
        }, imageReaderHandler);
        // Feed decoder until it has an output buffer
        final MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
        int outBufferIndex;
        while ((outBufferIndex = decoder.dequeueOutputBuffer(bufferInfo, 0)) < 0) {
            final int bufferIndex = decoder.dequeueInputBuffer(-1L);
            final ByteBuffer inputBuffer = decoder.getInputBuffer(bufferIndex);
            final int dataSize = extractor.readSampleData(inputBuffer, 0);
            final long sampleTime = extractor.getSampleTime();
            if (dataSize >= 0 && endTimestamp > sampleTime) {
                decoder.queueInputBuffer(bufferIndex, 0, dataSize, sampleTime, extractor.getSampleFlags());
                extractor.advance();
            } else return -1;
        }
        decoder.releaseOutputBuffer(outBufferIndex, true);
        // Wait for surface texture rendering
        try { renderCompletionToken.acquire(); } catch (InterruptedException ex) {}
        try { readbackCompletionToken.acquire(); } catch (InterruptedException ex) {}
        return timestamp.value;
    }

    public int pixelWidth () {
        return imageReader != null ? imageReader.getWidth() : 0;
    }

    public int pixelHeight () {
        return imageReader != null ? imageReader.getHeight() : 0;
    }

    public float frameRate () {
        return frameRate;
    }
    //endregion


    //region --Operations--

    private final MediaExtractor extractor;
    private final HandlerThread imageReaderThread = new HandlerThread("FrameReader");
    private long endTimestamp;
    private ImageReader imageReader;
    private Handler imageReaderHandler;
    private GLRenderContext renderContext;
    private Handler renderContextHandler;

    private int decoderOutputTextureID;
    private SurfaceTexture decoderOutputTexture;
    private Surface decoderOutputSurface;
    private GLBlitEncoder blitEncoder;
    private MediaCodec decoder;
    private float frameRate;

    private static MediaFormat videoFormat (MediaExtractor extractor) {
        // Search
        for (int i = 0; i < extractor.getTrackCount(); i++) {
            final MediaFormat format = extractor.getTrackFormat(i);
            if (format.getString(MediaFormat.KEY_MIME).startsWith("video/")) {
                extractor.selectTrack(i);
                return format;
            }
        }
        // Failed
        Log.e("Unity", "NatReader Error: Failed to find video track in media file");
        return null;
    }
    //endregion
}
