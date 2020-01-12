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

public final class MP4FrameReader implements FrameReader {

    //region --Client API--

    public MP4FrameReader (String uri, float startTime, float duration) {
        // Setup extractor
        this.uri = uri;
        this.extractor = new MediaExtractor();
        try {
            extractor.setDataSource(uri);
        } catch (IOException ex) {
            Log.e("NatSuite", "NatReader Error: MP4FrameReader failed to create media extractor with error: " + ex);
            return;
        }
        // Seek
        this.format = videoFormat(extractor);
        if (format == null) {
            Log.e("NatSuite", "NatReader Error: MP4FrameReader failed to find video track in media file");
            return;
        }
        long startTimeUs = (long)(startTime * 1e+6);
        extractor.seekTo(startTimeUs, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
        endTimestamp = startTimeUs + (long)(duration * 1e+6);
        // Setup image reader
        this.imageReaderThread.start();
        this.imageReaderHandler = new Handler(imageReaderThread.getLooper());
        this.imageReader = ImageReader.newInstance(frameWidth(), frameHeight(), PixelFormat.RGBA_8888, 2);
        // Setup render context
        this.renderContext = new GLRenderContext(null, imageReader.getSurface(), false);
        this.renderContext.start();
        this.renderContextHandler = new Handler(renderContext.getLooper());
        // Create decoder
        final Semaphore completionToken = new Semaphore(0);
        renderContextHandler.post(new Runnable() {
            @Override
            public void run() {
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
                }
                catch (Exception ex) { Log.e("NatSuite", "NatReader Error: MP4FrameReader failed to start decoder with error: " + ex); }
                finally { completionToken.release(); }
            }
        });
        try { completionToken.acquire(); } catch (InterruptedException ex) { }
        Log.d("NatSuite", "NatReader: Created MP4FrameReader for media at '" + uri + "' with size: " + frameWidth() + "x" + frameHeight());
    }

    @Override
    public String uri () {
        return uri;
    }

    @Override
    public float duration () {
        return format != null ? format.getLong(MediaFormat.KEY_DURATION) / 1e+6f : 0;
    }

    @Override
    public int frameWidth () {
        return format != null ? format.getInteger(MediaFormat.KEY_WIDTH) : 0;
    }

    @Override
    public int frameHeight () {
        return format != null ? format.getInteger(MediaFormat.KEY_HEIGHT) : 0;
    }

    @Override
    public float frameRate () {
        return format != null ? Build.VERSION.SDK_INT >= Build.VERSION_CODES.M ? format.getInteger(MediaFormat.KEY_FRAME_RATE) : 30 : 0; // Default to 30
    }

    @Override
    public long copyNextFrame (final ByteBuffer dstBuffer) {
        try {
            final Semaphore renderCompletionToken = new Semaphore(0);
            final Semaphore readbackCompletionToken = new Semaphore(0);
            final class Timestamp { long value = -1L; }
            final Timestamp timestamp = new Timestamp();
            decoderOutputTexture.setOnFrameAvailableListener(new SurfaceTexture.OnFrameAvailableListener() {
                @Override
                public void onFrameAvailable(SurfaceTexture surfaceTexture) {
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
                public void onImageAvailable(ImageReader imageReader) {
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
            renderCompletionToken.acquire();
            readbackCompletionToken.acquire();
            return timestamp.value;
        } catch (Exception ex) {
            Log.e("NatSuite", "NatReader Error: MP4FrameReader failed to copy frame with error: " + ex);
            dstBuffer.limit(0);
            return -1L;
        }
    }

    @Override
    public void reset () {
        extractor.seekTo(0, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
    }

    @Override
    public void release () {
        try {
            extractor.release();
            decoder.stop();
            decoder.release();
            renderContextHandler.post(new Runnable() {
                @Override
                public void run() {
                    blitEncoder.release();
                    decoderOutputSurface.release();
                    decoderOutputTexture.release();
                    GLBlitEncoder.releaseTexture(decoderOutputTextureID);
                }
            });
            renderContext.quitSafely();
            renderContext.join();
            imageReader.close();
            imageReaderThread.quitSafely();
        } catch (Exception ex) {
            Log.e("NatSuite", "NatReader Error: Failed to properly release MP4FrameReader");
        }
    }
    //endregion


    //region --Operations--

    private final String uri;
    private final MediaExtractor extractor;
    private final HandlerThread imageReaderThread = new HandlerThread("FrameReader");
    private long endTimestamp;
    private MediaFormat format;
    private ImageReader imageReader;
    private Handler imageReaderHandler;
    private GLRenderContext renderContext;
    private Handler renderContextHandler;

    private int decoderOutputTextureID;
    private SurfaceTexture decoderOutputTexture;
    private Surface decoderOutputSurface;
    private GLBlitEncoder blitEncoder;
    private MediaCodec decoder;

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
        return null;
    }
    //endregion
}
