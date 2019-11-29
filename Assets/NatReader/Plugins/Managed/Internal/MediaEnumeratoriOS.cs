/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba.
*/

namespace NatReader.Internal {

    using System;

    public sealed class MediaEnumeratoriOS : IMediaEnumerator {

        #region --IMediaEnumerator--

        public MediaEnumeratoriOS (IntPtr reader) => this.reader = reader;

        public void Dispose () => reader.Dispose();

        public bool CopyNextFrame (IntPtr dstBuffer, out int byteSize, out long timestamp) => reader.CopyNextFrame(dstBuffer, out byteSize, out timestamp);
        #endregion
        
        private readonly IntPtr reader;
    }
}