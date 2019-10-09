/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader {

    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Internal;

    /// <summary>
    /// Video frame reader.
    /// </summary>
    public sealed class FrameReader : IMediaReader<(byte[], long)> {

        #region --Client API--
        /// <summary>
        /// Media source URI
        /// </summary>
        public readonly string uri;
        /// <summary>
        /// Media pixel width
        /// </summary>
        public readonly int pixelWidth;

        /// <summary>
        /// Media pixel height
        /// </summary>
        public readonly int pixelHeight;

        /// <summary>
        /// Media frame rate
        /// </summary>
        public readonly float frameRate;

        /// <summary>
        /// Create a frame reader
        /// </summary>
        /// <param name="uri">URL to media source. MUST be prepended with URI scheme/protocol.</param>
        /// <param name="startTime">Media time to start reading samples in nanoseconds</param>
        public FrameReader (string uri, long startTime = 0) {
            // Create platform-specific reader
            this.uri = uri;
            switch (Application.platform) {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    goto case RuntimePlatform.IPhonePlayer;
                case RuntimePlatform.IPhonePlayer: {
                    var nativeReader = MediaReaderBridge.CreateFrameReader(uri, startTime);
                    nativeReader.GetProperties(out var pixelWidth, out var pixelHeight, out var frameRate);
                    this.reader = new MediaReaderiOS(nativeReader);
                    this.pixelWidth = pixelWidth;
                    this.pixelHeight = pixelHeight;
                    this.frameRate = frameRate;
                    break;
                }
                case RuntimePlatform.Android: {
                    var nativeReader = new AndroidJavaObject(@"com.olokobayusuf.natreader.FrameReader", uri, startTime);
                    this.reader = new MediaReaderAndroid(nativeReader);
                    this.pixelWidth = nativeReader.Call<int>(@"pixelWidth");
                    this.pixelHeight = nativeReader.Call<int>(@"pixelHeight");
                    this.frameRate = nativeReader.Call<float>(@"frameRate");
                    break;
                }
                default:
                    Debug.LogError("NatReader Error: FrameReader is not supported on this platform");
                    break;
            }
        }
        
        /// <summary>
        /// Release the reader
        /// </summary>
        public void Dispose () {
            reader.Dispose();
        }
        #endregion


        #region --Operations--

        private readonly INativeMediaReader reader;

        IEnumerator<(byte[], long)> IEnumerable<(byte[], long)>.GetEnumerator() {
            return GetNextFrame();
        }

        IEnumerator IEnumerable.GetEnumerator () {
            return (this as IEnumerable<(byte[], long)>).GetEnumerator();
        }

        IEnumerator<(byte[], long)> GetNextFrame () {
            var pixelBuffer = new byte[pixelWidth * pixelHeight * 4];
            for (;;) {
                var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                bool success = reader.CopyNextFrame(handle.AddrOfPinnedObject(), out var _,out var timestamp);
                handle.Free();
                if (success)
                    yield return (pixelBuffer, timestamp);
                else
                    break;
            }
        }
        #endregion
    }
}