/* 
*   NatReader
*   Copyright (c) 2021 Yusuf Olokoba.
*/

namespace NatSuite.Readers {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;
    using Internal;

    /// <summary>
    /// MP4 video frame reader.
    /// </summary>
    public sealed class MP4Reader : IFrameReader {

        #region --Client API--
        /// <summary>
        /// Media source path.
        /// </summary>
        public string path {
            get {
                var result = new StringBuilder(1024);
                reader.Path(result);
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
        /// <param name="path">Path to MP4 video file.</param>
        public MP4Reader (string path) => this.reader = Bridge.CreateMP4Reader(path);

        /// <summary>
        /// Read frames in a time range.
        /// </summary>
        /// <param name="startTime">Time to start reading samples in seconds.</param>
        /// <param name="duration">Duration in seconds. Pass `-1` to read till the end of the stream.</param>
        /// <param name="frameSkip">Number of frames to skip when reading.</param>
        public IEnumerable<(byte[] pixelBuffer, long timestamp)> Read (float startTime = 0, float duration = -1, int frameSkip = 0) {
            // Create enumerator
            var enumerator = reader.CreateEnumerator(startTime, duration, frameSkip);
            if (enumerator == IntPtr.Zero) {
                Debug.LogError("NatReader Error: MP4FrameReader failed to create enumerator");
                yield break;
            }
            // Read
            var nativeBuffer = Marshal.AllocHGlobal(frameSize.width * frameSize.height * 4); // Workaround for Android
            var pixelBuffer = new byte[frameSize.width * frameSize.height * 4];
            try {
                for (;;) {
                    enumerator.CopyNextFrame(nativeBuffer, out var _, out var timestamp);
                    Marshal.Copy(nativeBuffer, pixelBuffer, 0, pixelBuffer.Length);
                    if (timestamp < 0L) // EOS
                        break;
                    yield return (pixelBuffer, timestamp);
                }
            }
            // Dispose enumerator
            finally {
                Marshal.FreeHGlobal(nativeBuffer);
                enumerator.DisposeEnumerator();
            }
        }

        /// <summary>
        /// Dispose the reader and release resources.
        /// </summary>
        public void Dispose () => reader.Dispose();
        #endregion


        #region --Operations--

        private readonly IntPtr reader;

        public override string ToString () =>
        $"{{{Environment.NewLine}\t" +
        string.Join(
            $"{Environment.NewLine}\t",
            new Dictionary<string, object> {
                ["path"] = path,
                ["duration"] = duration,
                ["frameSize"] = frameSize,
                ["frameRate"] = frameRate
            }.Select(pair => $"{pair.Key}: {pair.Value}")
        ) +
        $"{Environment.NewLine}}}";
        #endregion
    }
}