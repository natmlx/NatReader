/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba.
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
    public sealed class FrameReader : IFrameReader {

        #region --Client API--
        /// <summary>
        /// Video source URI
        /// </summary>
        public readonly string uri;

        /// <summary>
        /// Video size
        /// </summary>
        public (int width, int height) frameSize {
            get; private set;
        }

        /// <summary>
        /// Video frame rate
        /// </summary>
        public float frameRate {
            get; private set;
        }
        
        /// <summary>
        /// Create a frame reader
        /// </summary>
        /// <param name="uri">URL to media source. MUST be prepended with URI scheme/protocol.</param>
        /// <param name="startTime">Optional. Media time to start reading samples in nanoseconds. Negative values read from end of media.</param>
        /// <param name="copyPixelBuffers">Optional. When false, a single pixel buffer is used so that no memory allocations are made during decoding.</param>
        public FrameReader (string uri, long startTime = 0) {
            // Save state
            this.uri = uri;
            // Create platform-specific reader
            switch (Application.platform) {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.IPhonePlayer: {
                    var nativeReader = MediaEnumeratorBridge.CreateFrameReader(uri, startTime);
                    nativeReader.GetProperties(out var pixelWidth, out var pixelHeight, out var frameRate);
                    this.enumerator = new MediaEnumeratoriOS(nativeReader);
                    this.frameSize = (pixelWidth, pixelHeight);
                    this.frameRate = frameRate;
                    break;
                }
                case RuntimePlatform.Android: {
                    var nativeReader = new AndroidJavaObject(@"api.natsuite.natreader.FrameReader", uri, startTime);
                    this.enumerator = new MediaEnumeratorAndroid(nativeReader);
                    this.frameSize = (nativeReader.Call<int>(@"pixelWidth"), nativeReader.Call<int>(@"pixelHeight"));
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
        public void Dispose () => enumerator.Dispose();
        #endregion


        #region --Operations--

        private readonly IMediaEnumerator enumerator;

        IEnumerator<(byte[] pixelBuffer, long timestamp)> IEnumerable<(byte[] pixelBuffer, long timestamp)>.GetEnumerator() {
            var pixelBuffer = new byte[frameSize.width * frameSize.height * 4];
            for (;;) {
                var handle = GCHandle.Alloc(pixelBuffer, GCHandleType.Pinned);
                bool success = enumerator.CopyNextFrame(handle.AddrOfPinnedObject(), out var _, out var timestamp);
                handle.Free();
                if (success)
                    yield return (pixelBuffer, timestamp);
                else
                    break;
            }
        }

        IEnumerator IEnumerable.GetEnumerator () => (this as IEnumerable<(byte[], long)>).GetEnumerator();
        #endregion
    }
}