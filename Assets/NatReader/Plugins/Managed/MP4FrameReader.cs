/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatReader {

    using System;
    using System.Collections;
    using System.Collections.Generic;
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
        /// <param name="startTime">Optional. Media time to start reading samples in seconds.</param>
        /// <param name="duration">Optional. Duration in seconds.</param>
        public MP4FrameReader (string uri, float startTime = 0f, float duration = float.PositiveInfinity) => this.reader = NativeBridge.CreateMP4FrameReader(uri, startTime, duration);
        
        /// <summary>
        /// Dispose the reader and release resources.
        /// </summary>
        public void Dispose () => reader.Dispose();
        #endregion


        #region --Operations--

        private readonly IntPtr reader;

        IEnumerator<(byte[] pixelBuffer, long timestamp)> IEnumerable<(byte[] pixelBuffer, long timestamp)>.GetEnumerator() {
            var pixelBuffer = new byte[frameSize.width * frameSize.height * 4];
            for (;;) {
                var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                bool success = reader.CopyNextFrame(handle.AddrOfPinnedObject(), out var _, out var timestamp);
                handle.Free();
                if (success)
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