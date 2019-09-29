/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader {

    using System;

    /// <summary>
    /// A reader capable of reading video frames from a media input.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public interface IMediaReader : IDisposable {
        /// <summary>
        /// Media pixel width
        /// </summary>
        int pixelWidth { get; }
        /// <summary>
        /// Media pixel height
        /// </summary>
        int pixelHeight { get; }
        /// <summary>
        /// Start reading frames
        /// </summary>
        /// <param name="frameHandler">Delegate invoked with new video frames</param>
        void StartReading (FrameHandler frameHandler);
    }

    public delegate void FrameHandler (IntPtr pixelBuffer, long timestamp);
}