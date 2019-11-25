/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba.
*/

namespace NatReader.Internal {

    using UnityEngine;
    using System;
    using System.Runtime.InteropServices;

    public sealed class MediaEnumeratorAndroid : IMediaEnumerator {

        #region --IMediaEnumerator--

        public MediaEnumeratorAndroid (AndroidJavaObject reader) {
            this.reader = reader;
            this.Unmanaged = new AndroidJavaClass(@"com.natsuite.natrender.Unmanaged");
        }

        public void Dispose () => reader.Call(@"release");

        public bool CopyNextFrame (IntPtr dstBuffer, out int dstSize, out long timestamp) {
            var sampleBuffer = reader.Call<AndroidJavaObject>(@"copyNextFrame");
            timestamp = sampleBuffer.Get<long>(@"timestamp");
            try {
                var buffer = sampleBuffer.Get<AndroidJavaObject>(@"buffer");
                dstSize = buffer.Call<int>(@"capacity");
                var srcBuffer = (IntPtr)Unmanaged.CallStatic<long>(@"baseAddress", buffer);
                memcpy(dstBuffer, srcBuffer, (UIntPtr)dstSize);
                buffer.Dispose();
                return true;
            } catch {
                dstSize = 0;
                return false;
            } finally {
                sampleBuffer.Dispose();
            }
        }
        #endregion


        #region --Operations--

        private readonly AndroidJavaObject reader;
        private readonly AndroidJavaClass Unmanaged;

        #if UNITY_ANDROID && !UNITY_EDITOR
        [DllImport(@"c")]
        private static extern IntPtr memcpy (IntPtr dst, IntPtr src, UIntPtr size);
        #else
        private static IntPtr memcpy (IntPtr dst, IntPtr src, UIntPtr size) => dst;
        #endif
        #endregion
    }
}