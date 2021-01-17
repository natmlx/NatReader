/* 
*   NatReader
*   Copyright (c) 2021 Yusuf Olokoba.
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
        /// Media source path.
        /// </summary>
        string path { get; }

        /// <summary>
        /// Media duration in seconds.
        /// </summary>
        float duration { get; }

        /// <summary>
        /// Read frames in a time range.
        /// </summary>
        /// <param name="startTime">Time to start reading samples in seconds.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="frameSkip">Number of frames to skip when reading.</param>
        IEnumerable<T> Read (float startTime = 0, float duration = -1, int frameSkip = 0);
    }
}