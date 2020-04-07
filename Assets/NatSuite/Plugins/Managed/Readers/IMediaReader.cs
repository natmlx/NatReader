/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Readers {

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A reader capable of reading media frames from a media input.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public interface IMediaReader <T> : IDisposable {

        /// <summary>
        /// Media source URI.
        /// </summary>
        string uri { get; }

        /// <summary>
        /// Media duration in seconds.
        /// </summary>
        float duration { get; }

        /// <summary>
        /// Read frames in a time range.
        /// </summary>
        /// <param name="startTime">Optional. Time to start reading samples in seconds.</param>
        /// <param name="duration">Optional. Duration in seconds.</param>
        IEnumerable<T> Read (float startTime = 0, float duration = -1);
    }

    /// <summary>
    /// A reader capable of reading video frames from a video input.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    public interface IFrameReader : IMediaReader<(byte[] pixelBuffer, long timestamp)> {

        /// <summary>
        /// Frame size.
        /// </summary>
        (int width, int height) frameSize { get; }

        /// <summary>
        /// Frame rate.
        /// </summary>
        float frameRate { get; }
    }
}