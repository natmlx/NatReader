package com.olokobayusuf.natreader;

import java.nio.ByteBuffer;

public interface MediaReader {

    /**
     * Media pixel width
     */
    int pixelWidth ();

    /**
     * Media pixel height
     */
    int pixelHeight ();

    /**
     * Start reading media samples from a given URL
     * @param callback Callback invoked with new frames
     */
    void startReading (Callback callback);

    /**
     * Stop reading and release the reader
     */
    void release ();

    interface Callback {
        /**
         * Delegate invoked with new video frames
         * @param pixelBuffer Pixel buffer containing frame in RGBA32 layout
         * @param timestamp Frame timestamp in nanoseconds
         */
        void onFrame (ByteBuffer pixelBuffer, long timestamp);
    }
}
