package com.natsuite.natreader;

import java.nio.ByteBuffer;

public interface MediaReader {

    final class SampleBuffer {
        public ByteBuffer buffer;
        public long timestamp;
        public final static SampleBuffer EOS = new SampleBuffer();
    }

    SampleBuffer copyNextFrame ();

    void release ();
}
