/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Internal {

    using UnityEngine;
    using UnityEngine.Scripting;
    using System;

    public sealed class MediaReaderAndroid : AndroidJavaProxy, IMediaReader {

        #region --IMediaReader--

        public int pixelWidth {
            get { return reader.Call<int>(@"pixelWidth"); }
        }

        public int pixelHeight {
            get { return reader.Call<int>(@"pixelHeight"); }
        }

        public MediaReaderAndroid (AndroidJavaObject reader) : base("com.olokobayusuf.natreader.MediaReader$Callback") {
            this.reader = reader;
            this.Unmanaged = new AndroidJavaClass(@"com.olokobayusuf.natrender.Unmanaged");
        }

        public void Dispose () {
            reader.Call(@"release");
            reader.Dispose();
            frameHandler = null;
        }

        public void StartReading (FrameHandler frameHandler) {
            this.frameHandler = frameHandler;
            reader.Call(@"startReading", this);
        }
        #endregion


        #region --Operations--

        private readonly AndroidJavaObject reader;
        private readonly AndroidJavaClass Unmanaged;
        private FrameHandler frameHandler;

        [Preserve]
        private void onFrame (AndroidJavaObject nativeBuffer, long timestamp) {
            var pixelBuffer = (IntPtr)Unmanaged.CallStatic<long>(@"baseAddress", nativeBuffer);
            nativeBuffer.Dispose();
            frameHandler(pixelBuffer, timestamp);
        }
        #endregion
    }
}