/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba.
*/

namespace NatReader {

    /// <summary>
    /// A reader capable of reading video frames from a video input.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public interface IFrameReader : IMediaReader<(byte[] pixelBuffer, long timestamp)> {

        /// <summary>
        /// Video size
        /// </summary>
        (int width, int height) frameSize { get; }

        /// <summary>
        /// Frame rate
        /// </summary>
        float frameRate { get; }
    }
}