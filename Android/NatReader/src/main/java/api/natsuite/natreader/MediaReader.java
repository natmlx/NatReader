package api.natsuite.natreader;

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
     * Create an enumerator for reading media samples from the reader
     * @param startTime Read start time in seconds.
     * @param duration Read duration in seconds.
     * @param frameSkip Number of frames to skip when reading.
     */
    MediaEnumerator createEnumerator (float startTime, float duration, int frameSkip);

    /**
     * Release the reader and any resources.
     */
    void release ();
}
