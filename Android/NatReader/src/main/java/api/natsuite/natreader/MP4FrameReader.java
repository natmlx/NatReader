package api.natsuite.natreader;

import android.media.MediaExtractor;
import android.media.MediaFormat;
import android.os.Build;
import android.util.Log;
import java.io.IOException;

public final class MP4FrameReader implements FrameReader {

    //region --Client API--

    public MP4FrameReader (String uri) {
        this.uri = uri;
        this.extractor = new MediaExtractor();
        MediaFormat format = null;
        try {
            extractor.setDataSource(uri);
            format = videoFormat(extractor);
            if (format == null)
                Log.e("NatSuite", "NatReader Error: MP4FrameReader failed to find video track in media file");
        } catch (IOException ex) {
            Log.e("NatSuite", "NatReader Error: MP4FrameReader failed to create media extractor with error: " + ex);
        }
        this.format = format;
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
    public MediaEnumerator createEnumerator (float startTime, float duration) {
        return new FrameEnumerator2(extractor, format, startTime, duration);
    }

    @Override
    public void release () {
        extractor.release();
    }
    //endregion


    //region --Operations--

    private final String uri;
    private final MediaExtractor extractor;
    private final MediaFormat format;

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
