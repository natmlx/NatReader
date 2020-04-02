package api.natsuite.natreader;

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
import api.natsuite.natrender.GLBlitEncoder;
import api.natsuite.natrender.GLRenderContext;

final class FrameEnumerator2 implements MediaEnumerator { // INCOMPLETE // DEPLOY

    private final MediaExtractor extractor;
    private final ImageReader imageReader;
    private long endTimestamp;
    private final Semaphore feedFence = new Semaphore(0);
    private final Semaphore readFence = new Semaphore(0);

    public FrameEnumerator2 (final MediaExtractor extractor, final MediaFormat format, final float startTime, final float duration) {
        // Set time range
        final long startTimeUs = (long)(startTime * 1e+6);
        this.extractor = extractor;
        this.endTimestamp = startTimeUs + (long)(duration * 1e+6);
        extractor.seekTo(startTimeUs, MediaExtractor.SEEK_TO_CLOSEST_SYNC);
        // Create image reader
        final HandlerThread imageReaderThread = new HandlerThread("Frame Enumerator Thread");
        imageReaderThread.start();
        final Handler imageReaderHandler = new Handler(imageReaderThread.getLooper());
        this.imageReader = ImageReader.newInstance(format.getInteger(MediaFormat.KEY_WIDTH), format.getInteger(MediaFormat.KEY_HEIGHT), PixelFormat.RGBA_8888, 2);
        this.imageReader.setOnImageAvailableListener(unused -> readFence.release(), imageReaderHandler);
        // Setup render context
        final GLRenderContext renderContext = new GLRenderContext(null, imageReader.getSurface(), false);
        renderContext.start();
        final Handler renderContextHandler = new Handler(renderContext.getLooper());
        // Create decoder
        renderContextHandler.post(() -> {
            // Create output texture
            final int decoderOutputTextureID = GLBlitEncoder.getExternalTexture();
            final SurfaceTexture decoderOutputTexture = new SurfaceTexture(decoderOutputTextureID);
            final Surface decoderOutputSurface = new Surface(decoderOutputTexture);
            final GLBlitEncoder blitEncoder = GLBlitEncoder.externalBlitEncoder();
            // Set frame handler
            decoderOutputTexture.setOnFrameAvailableListener(
                surfaceTexture -> {
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
                },
                renderContextHandler
            );
            // Decoding thread
            new Thread(() -> {
                // Create decoder
                final MediaCodec.BufferInfo bufferInfo = new MediaCodec.BufferInfo();
                final MediaCodec decoder;
                try {
                    decoder = MediaCodec.createDecoderByType(format.getString(MediaFormat.KEY_MIME));
                    decoder.configure(format, decoderOutputSurface, null, 0);
                    decoder.start();
                }
                catch (Exception ex) {
                    Log.e("NatSuite", "NatReader Error: FrameEnumerator failed to create decoder with error: " + ex);
                    readFence.release();
                    return;
                }
                // Feed decoder
                while (true) {
                    // Check for new frame
                    int outBufferIndex = decoder.dequeueOutputBuffer(bufferInfo, 0L);
                    if (outBufferIndex >= 0) {
                        decoder.releaseOutputBuffer(outBufferIndex, true);
                        try { feedFence.acquire(); } catch (InterruptedException ex) { }
                    }
                    // Dequeue
                    final int bufferIndex = decoder.dequeueInputBuffer(-1L);
                    final ByteBuffer inputBuffer = decoder.getInputBuffer(bufferIndex);
                    final int dataSize = extractor.readSampleData(inputBuffer, 0);
                    final long sampleTime = extractor.getSampleTime();
                    // Check for EOS
                    if (dataSize < 0 || sampleTime > endTimestamp)
                        break;
                    // Commit
                    decoder.queueInputBuffer(bufferIndex, 0, dataSize, sampleTime, extractor.getSampleFlags());
                    extractor.advance();
                }
                // Release decoder and everything else
                decoder.stop();
                decoder.release();
                renderContextHandler.post(() -> {
                    blitEncoder.release();
                    decoderOutputSurface.release();
                    decoderOutputTexture.release();
                    GLBlitEncoder.releaseTexture(decoderOutputTextureID);
                });
                renderContext.quitSafely();
                imageReader.close();
                imageReaderThread.quitSafely();
            }).start();
        });
    }

    @Override
    public long copyNextFrame (final ByteBuffer dstBuffer) {
        try {
            readFence.acquire();
            feedFence.release();
            final Image image = imageReader.acquireLatestImage();
            if (image == null) {
                dstBuffer.limit(0);
                return -1L;
            }
            final Image.Plane imagePlane = image.getPlanes()[0];
            final ByteBuffer srcBuffer = imagePlane.getBuffer();
            final long timestamp = image.getTimestamp();
            Log.d("NatSuite", "capacity: "+dstBuffer.capacity()+" pos: "+dstBuffer.position()+" lim: "+dstBuffer.limit());
            GLBlitEncoder.copyFrame(srcBuffer, image.getWidth(), image.getHeight(), imagePlane.getRowStride(), dstBuffer);
            dstBuffer.rewind();
            dstBuffer.limit(image.getWidth() * image.getHeight() * 4);
            image.close();
            return timestamp;
        } catch (Exception ex) {
            Log.e("NatSuite", "FrameEnumerator encountered error when copying frame", ex);
            return -1L;
        }
    }

    public void release () {
        endTimestamp = extractor.getSampleTime() - 10L; // Nice little trick
        feedFence.release();
    }
}
