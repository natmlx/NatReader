/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Internal {

    using UnityEngine;
    using System;

    public sealed class MediaReaderAndroid : INativeMediaReader { // INCOMPLETE

        #region --IMediaReader--

        public MediaReaderAndroid (AndroidJavaObject reader) {
            this.reader = reader;
        }

        public void Dispose () {

        }

        public bool CopyNextFrame (IntPtr dstBuffer, out int dstSize, out long timestamp) {
            timestamp = 0;
            dstSize = 0;
            return false;
        }
        #endregion

        private readonly AndroidJavaObject reader;
    }
}