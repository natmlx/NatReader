package api.natsuite.natreader;

public interface FrameReader extends MediaReader {

    /**
     * Frame width.
     */
    int frameWidth ();

    /**
     * Frame height.
     */
    int frameHeight ();
}
