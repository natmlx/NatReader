/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Internal {

    using System;

    public sealed class MediaReaderiOS : INativeMediaReader {

        #region --INativeMediaReader--

        public MediaReaderiOS (IntPtr reader) {
            this.reader = reader;
        }

        public void Dispose () {
            reader.Dispose();
        }

        public bool CopyNextFrame (IntPtr dstBuffer, out int byteSize, out long timestamp) {
            return reader.CopyNextFrame(dstBuffer, out byteSize, out timestamp);
        }
        #endregion
        
        private readonly IntPtr reader;
    }
}