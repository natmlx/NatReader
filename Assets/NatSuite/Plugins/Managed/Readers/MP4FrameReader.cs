/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Readers {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Internal;

    /// <summary>
    /// MP4 video frame reader.
    /// </summary>
    public sealed class MP4FrameReader : IFrameReader {

        #region --Client API--
        /// <summary>
        /// Media source URI.
        /// </summary>
        public string uri {
            get {
                var result = new StringBuilder(1024);
                reader.URI(result);
                return result.ToString();
            }
        }

        /// <summary>
        /// Media duration in seconds.
        /// </summary>
        public float duration => reader.Duration();

        /// <summary>
        /// Frame size.
        /// </summary>
        public (int width, int height) frameSize {
            get {
                reader.FrameSize(out var width, out var height);
                return (width, height);
            }
        }

        /// <summary>
        /// Frame rate.
        /// </summary>
        public float frameRate => reader.FrameRate();
        
        /// <summary>
        /// Create an MP4 frame reader.
        /// </summary>
        /// <param name="uri">URL to media source. MUST be prepended with URI scheme/protocol.</param>
        public MP4FrameReader (string uri) => this.reader = Bridge.CreateMP4FrameReader(uri);
        
        /// <summary>
        /// Dispose the reader and release resources.
        /// </summary>
        public void Dispose () => reader.Dispose();

        /// <summary>
        /// Read frames in a time range.
        /// </summary>
        /// <param name="startTime">Time to start reading samples in seconds.</param>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="frameSkip">Number of frames to skip when reading.</param>
        public IEnumerable<(byte[] pixelBuffer, long timestamp)> Read (float startTime = 0, float duration = -1, int frameSkip = 0) {
            // Create enumerator
            var enumerator = reader.CreateEnumerator(startTime, duration, frameSkip);
            if (enumerator == IntPtr.Zero)
                yield break;
            // Read
            try {
                for (var pixelBuffer = new byte[frameSize.width * frameSize.height * 4];;) {
                    var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                    enumerator.CopyNextFrame(handle.AddrOfPinnedObject(), out var _, out var timestamp);
                    handle.Free();
                    if (timestamp < 0L)
                        break;
                    yield return (pixelBuffer, timestamp);
                }
            }
            // Dispose enumerator
            finally {
                enumerator.DisposeEnumerator();
            }
        }
        #endregion


        #region --Operations--

        private readonly IntPtr reader;

        public override string ToString () =>
        $"{{{Environment.NewLine}\t" +
        string.Join(
            $"{Environment.NewLine}\t",
            new Dictionary<string, object> {
                ["uri"] = uri,
                ["duration"] = duration,
                ["frameSize"] = frameSize,
                ["frameRate"] = frameRate
            }.Select(pair => $"{pair.Key}: {pair.Value}")
        ) +
        $"{Environment.NewLine}}}";
        #endregion
    }
}