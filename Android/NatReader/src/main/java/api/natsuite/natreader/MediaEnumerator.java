package api.natsuite.natreader;

import java.nio.ByteBuffer;

public interface MediaEnumerator {

    /**
     * Copy a frame into a destination buffer.
     * @param destination Destination buffer. The limit will be set to the number of bytes copied.
     * @return Timestamp of the frame, or -1 for EOS.
     */
    long copyNextFrame (ByteBuffer destination);

    /**
     * Release the enumerator and any resources.
     */
    void release ();
}
