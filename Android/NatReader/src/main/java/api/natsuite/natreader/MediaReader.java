package api.natsuite.natreader;

import java.nio.ByteBuffer;

public interface MediaReader {

    /**
     * Copy a frame into a destination buffer
     * @param destination Destination buffer. The limit will be set to the number of bytes copied
     * @return Timestamp of the frame, or -1 for EOS.
     */
    long copyNextFrame (ByteBuffer destination);

    /**
     * Release the reader and any resources
     */
    void release ();
}
