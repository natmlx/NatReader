package api.natsuite.natreader;

import java.nio.ByteBuffer;

public interface MediaReader {

    /**
     * Media source URI.
     */
    String uri ();

    /**
     * Media duration in seconds.
     */
    float duration ();

    /**
     * Copy a frame into a destination buffer.
     * @param destination Destination buffer. The limit will be set to the number of bytes copied.
     * @return Timestamp of the frame, or -1 for EOS.
     */
    long copyNextFrame (ByteBuffer destination);

    /**
     * Reset the media reader to start reading from the beginning of the media source.
     */
    void reset ();

    /**
     * Release the reader and any resources.
     */
    void release ();
}
