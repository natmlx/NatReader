/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Readers {

    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using Internal;

    /// <summary>
    /// MP4 video frame reader.
    /// </summary>
    [Doc(@"MP4FrameReader")]
    public sealed class MP4FrameReader : IFrameReader {

        #region --Client API--
        /// <summary>
        /// Media source URI.
        /// </summary>
        [Doc(@"URI")]
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
        [Doc(@"Duration")]
        public float duration => reader.Duration();

        /// <summary>
        /// Frame size.
        /// </summary>
        [Doc(@"FrameSize")]
        public (int width, int height) frameSize {
            get {
                reader.FrameSize(out var width, out var height);
                return (width, height);
            }
        }

        /// <summary>
        /// Frame rate.
        /// </summary>
        [Doc(@"FrameRate")]
        public float frameRate => reader.FrameRate();
        
        /// <summary>
        /// Create an MP4 frame reader.
        /// </summary>
        /// <param name="uri">URL to media source. MUST be prepended with URI scheme/protocol.</param>
        [Doc(@"MP4FrameReaderCtor")]
        public MP4FrameReader (string uri) => this.reader = Bridge.CreateMP4FrameReader(uri);
        
        /// <summary>
        /// Dispose the reader and release resources.
        /// </summary>
        [Doc(@"Dispose")]
        public void Dispose () => reader.Dispose();

        /// <summary>
        /// Read frames in a time range.
        /// </summary>
        /// <param name="startTime">Optional. Time to start reading samples in seconds.</param>
        /// <param name="duration">Optional. Duration in seconds.</param>
        [Doc(@"Read")]
        public IEnumerable<(byte[] pixelBuffer, long timestamp)> Read (float startTime = 0, float duration = -1) {
            var enumerator = reader.GetEnumerator(startTime, Math.Max(duration, this.duration));
            var pixelBuffer = new byte[frameSize.width * frameSize.height * 4];
            for (;;) {
                var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                enumerator.CopyNextFrame(handle.AddrOfPinnedObject(), out var _, out var timestamp);
                handle.Free();
                if (timestamp < 0L)
                    break;
                yield return (pixelBuffer, timestamp);
            }
            enumerator.DisposeEnumerator();
        }
        #endregion

        private readonly IntPtr reader;
    }
}