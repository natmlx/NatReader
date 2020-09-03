package api.natsuite.natreader.enumerators;

import android.graphics.PixelFormat;
import android.graphics.SurfaceTexture;
import android.media.Image;
import android.media.ImageReader;
import android.media.MediaCodec;
import android.media.MediaExtractor;
import android.media.MediaFormat;
import android.opengl.Matrix;
import android.os.Handler;
import android.os.HandlerThread;
import android.util.Log;
import android.view.Surface;
import java.nio.ByteBuffer;
import java.util.concurrent.Semaphore;

import api.natsuite.natreader.MediaEnumerator;
import api.natsuite.natrender.GLBlitEncoder;
import api.natsuite.natrender.GLRenderContext;

public final class FrameEnumerator2 implements MediaEnumerator {

    private final MediaExtractor extractor;
    private final HandlerThread imageReaderThread;
    private final long endTimestamp;
    private ImageReader imageReader;
    private Handler imageReaderHandler;
    private GLRenderContext renderContext;
    private Handler renderContextHandler;

    private int decoderOutputTextureID;
    private SurfaceTexture decoderOutputTexture;
    private Surface decoderOutputSurface;
    private GLBlitEncoder blitEncoder;
    private MediaCodec decoder;

    public FrameEnumerator2 (final MediaExtractor extractor, final MediaFormat format, final float startTime, final float duration) {
        // Set time range
        final long startTimeUs = (long)(startTime * 1e+6);
        this.extractor = extractor;
        this.endTimestamp = startTimeUs + (long)(duration * 1e+6);
        extractor.seekTo(startTimeUs, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
        // Create image reader
        this.imageReaderThread = new HandlerThread("Frame Enumerator Thread");
        this.imageReaderThread.start();
        this.imageReaderHandler = new Handler(imageReaderThread.getLooper());
        this.imageReader = ImageReader.newInstance(format.getInteger(MediaFormat.KEY_WIDTH), format.getInteger(MediaFormat.KEY_HEIGHT), PixelFormat.RGBA_8888, 2);
        // Setup render context
        this.renderContext = new GLRenderContext(null, imageReader.getSurface(), false);
        this.renderContext.start();
        this.renderContextHandler = new Handler(renderContext.getLooper());
        // Create decoder
        final Semaphore completionToken = new Semaphore(0);
        renderContextHandler.post(() -> {
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
            catch (Exception ex) { Log.e("NatSuite", "NatReader Error: FrameEnumerator failed to create decoder with error: " + ex); }
            finally { completionToken.release(); }
        });
        try { completionToken.acquire(); } catch (InterruptedException ex) { }
    }

    @Override
    public long copyNextFrame (final ByteBuffer dstBuffer) {
        try {
            final Semaphore renderCompletionToken = new Semaphore(0);
            final Semaphore readbackCompletionToken = new Semaphore(0);
            final class Timestamp { long value = -1L; }
            final Timestamp timestamp = new Timestamp();
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
                // Signal completion
                renderCompletionToken.release();
            }, renderContextHandler);
            imageReader.setOnImageAvailableListener(imageReader -> {
                // Create contiguous buffer with no padding
                final Image image = imageReader.acquireLatestImage();
                if (image != null) {
                    final Image.Plane imagePlane = image.getPlanes()[0];
                    final ByteBuffer srcBuffer = imagePlane.getBuffer();
                    final int width = image.getWidth();
                    final int height = image.getHeight();
                    final int stride = imagePlane.getRowStride();
                    GLBlitEncoder.copyFrame(srcBuffer, width, height, stride, dstBuffer);
                    dstBuffer.rewind();
                    dstBuffer.limit(width * height * 4);
                    timestamp.value = image.getTimestamp();
                    image.close();
                }
                // Send to waiter
                readbackCompletionToken.release();
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
    public void release () {
        try {
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
}
