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
     */
    MediaEnumerator createEnumerator (float startTime, float duration);

    /**
     * Release the reader and any resources.
     */
    void release ();
}
