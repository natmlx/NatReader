/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Readers {

    using System;
    using System.Collections.Generic;
    using Internal;

    /// <summary>
    /// A reader capable of reading media frames from a media input.
    /// All recorder methods are thread safe, and as such can be called from any thread.
    /// </summary>
    [Doc(@"IMediaReader")]
    public interface IMediaReader <T> : IDisposable {

        /// <summary>
        /// Media source URI.
        /// </summary>
        [Doc(@"URI")]
        string uri { get; }

        /// <summary>
        /// Media duration in seconds.
        /// </summary>
        [Doc(@"Duration")]
        float duration { get; }

        /// <summary>
        /// Read frames in a time range.
        /// </summary>
        /// <param name="startTime">Optional. Time to start reading samples in seconds.</param>
        /// <param name="duration">Optional. Duration in seconds.</param>
        [Doc(@"Read")]
        IEnumerable<T> Read (float startTime = 0, float duration = -1);
    }

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