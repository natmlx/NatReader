/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader {

    using UnityEngine;
    using Internal;

    /// <summary>
    /// Video frame reader.
    /// </summary>
    public sealed class FrameReader : IMediaReader {

        #region --Client API--

        /// <summary>
        /// Media pixel width
        /// </summary>
        public int pixelWidth {
            get { return reader.pixelWidth; }
        }

        /// <summary>
        /// Media pixel height
        /// </summary>
        public int pixelHeight {
            get { return reader.pixelHeight; }
        }
        
        /// <summary>
        /// Create a frame reader
        /// </summary>
        /// <param name="uri">URL to media source. MUST be prepended with URI scheme/protocol.</param>
        public FrameReader (string uri) {
            switch (Application.platform) {
                case RuntimePlatform.Android: {
                    var nativeReader = new AndroidJavaObject(@"com.olokobayusuf.natreader.FrameReader", uri);
                    this.reader = new MediaReaderAndroid(nativeReader);
                    break;
                }
                case RuntimePlatform.IPhonePlayer: {
                    var nativeReader = MediaReaderBridge.CreateFrameReader(uri);
                    this.reader = new MediaReaderiOS(nativeReader);
                    break;
                }
                default:
                    Debug.LogError("NatReader Error: FrameReader is not supported on this platform");
                    break;
            }
        }

        /// <summary>
        /// Start reading frames
        /// </summary>
        /// <param name="frameHandler">Delegate invoked with new video frames</param>
        public void StartReading (FrameHandler frameHandler) {
            reader.StartReading(frameHandler);
        }

        /// <summary>
        /// Stop reading and release the reader
        /// </summary>
        public void Dispose () {
            reader.Dispose();
        }
        #endregion

        private readonly IMediaReader reader;
    }
}