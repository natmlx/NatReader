/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba.
*/

namespace NatReader.Internal {

    using UnityEngine;
    using System;

    public sealed class MediaEnumeratorAndroid : IMediaEnumerator {

        #region --IMediaEnumerator--

        public MediaEnumeratorAndroid (AndroidJavaObject reader) {
            this.reader = reader;
            this.Unmanaged = new AndroidJavaClass(@"api.natsuite.natrender.Unmanaged");
        }

        public void Dispose () => reader.Call(@"release");

        public bool CopyNextFrame (IntPtr dstBuffer, out int dstSize, out long timestamp) { // DEPLOY
            using (var pixelBuffer = Unmanaged.CallStatic<AndroidJavaObject>(@"wrapBuffer", (long)dstBuffer, IntPtr.Size)) {
                timestamp = reader.Call<long>(@"copyNextFrame", pixelBuffer);
                dstSize = pixelBuffer.Call<int>(@"limit");
                return timestamp > 0;
            }
        }
        #endregion


        #region --Operations--
        private readonly AndroidJavaObject reader;
        private readonly AndroidJavaClass Unmanaged;
        #endregion
    }
}