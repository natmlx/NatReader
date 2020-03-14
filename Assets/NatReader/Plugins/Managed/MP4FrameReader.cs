/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Readers {

    using System;
    using System.Collections;
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
        /// <param name="startTime">Optional. Media time to start reading samples in seconds.</param>
        /// <param name="duration">Optional. Duration in seconds.</param>
        [Doc(@"MP4FrameReaderCtor")]
        public MP4FrameReader (string uri, float startTime = 0f, float duration = 1e+6f) => this.reader = Bridge.CreateMP4FrameReader(uri, startTime, duration);
        
        /// <summary>
        /// Dispose the reader and release resources.
        /// </summary>
        [Doc(@"Dispose")]
        public void Dispose () => reader.Dispose();
        #endregion


        #region --Operations--

        private readonly IntPtr reader;

        IEnumerator<(byte[] pixelBuffer, long timestamp)> IEnumerable<(byte[] pixelBuffer, long timestamp)>.GetEnumerator() {
            var pixelBuffer = new byte[frameSize.width * frameSize.height * 4];
            for (;;) {
                // Copy
                var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                var bufferSize = pixelBuffer.Length; // In-out param
                reader.CopyNextFrame(handle.AddrOfPinnedObject(), out bufferSize, out var timestamp);
                handle.Free();
                // Check success
                if (timestamp >= 0L) // CHECK // Switch on timestamp or buffer size??
                    yield return (pixelBuffer, timestamp);
                else
                    break;
            }
            reader.Reset();
        }

        IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<(byte[], long)>).GetEnumerator();
        #endregion
    }
}