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
        /// Video frame width
        /// </summary>
        int pixelWidth { get; }

        /// <summary>
        /// Video frame height
        /// </summary>
        int pixelHeight { get; }
    }
}