/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatReader {

    using Internal;

    /// <summary>
    /// A reader capable of reading video frames from a video input.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    [Doc(@"IFrameReader")]
    public interface IFrameReader : IMediaReader<(byte[] pixelBuffer, long timestamp)> {

        /// <summary>
        /// Frame size.
        /// </summary>
        [Doc(@"FrameSize")]
        (int width, int height) frameSize { get; }

        /// <summary>
        /// Frame rate.
        /// </summary>
        [Doc(@"FrameRate")]
        float frameRate { get; }
    }
}