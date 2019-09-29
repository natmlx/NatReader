/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Internal {

    using AOT;
    using System;
    using System.Runtime.InteropServices;

    public sealed class MediaReaderiOS : IMediaReader {

        #region --IMediaReader--

        public int pixelWidth {
            get { return reader.PixelWidth(); }
        }

        public int pixelHeight {
            get { return reader.PixelHeight(); }
        }

        public MediaReaderiOS (IntPtr reader) {
            this.reader = reader;
        }

        public void StartReading (FrameHandler frameHandler) {
            this.self = GCHandle.Alloc(this, GCHandleType.Normal);
            this.frameHandler = frameHandler;
            reader.StartReading(OnFrame, (IntPtr)self);
        }

        public void Dispose () {
            reader.Dispose();
            self.Free();
        }
        #endregion


        #region --Operations--

        private readonly IntPtr reader;
        private GCHandle self;
        private FrameHandler frameHandler;

        [MonoPInvokeCallback(typeof(MediaReaderBridge.FrameHandler))]
        private static void OnFrame (IntPtr context, IntPtr pixelBuffer, long timestamp) {
            var instanceHandle = (GCHandle)context;
            var instance = instanceHandle.Target as MediaReaderiOS;
            instance.frameHandler(pixelBuffer, timestamp);
        }
        #endregion
    }
}