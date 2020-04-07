/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Readers.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Bridge {

        private const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatReader";
        #endif


        #region --MediaReader--
        [DllImport(Assembly, EntryPoint = @"NRDisposeReader")]
        public static extern void Dispose (this IntPtr reader);
        [DllImport(Assembly, EntryPoint = @"NRMediaURI")]
        public static extern void URI (this IntPtr reader, [MarshalAs(UnmanagedType.LPStr)] StringBuilder dstString);
        [DllImport(Assembly, EntryPoint = @"NRMediaDuration")]
        public static extern float Duration (this IntPtr reader);
        [DllImport(Assembly, EntryPoint = @"NRCreateEnumerator")]
        public static extern IntPtr CreateEnumerator (this IntPtr reader, float startTime, float duration);
        #endregion


        #region --MediaEnumerator--
        [DllImport(Assembly, EntryPoint = @"NRDisposeEnumerator")]
        public static extern void DisposeEnumerator (this IntPtr enumerator);
        [DllImport(Assembly, EntryPoint = @"NRCopyNextFrame")]
        public static extern void CopyNextFrame (this IntPtr enumerator, IntPtr buffer, out int bufferSize, out long timestamp);
        #endregion


        #region --FrameReader--
        [DllImport(Assembly, EntryPoint = @"NRCreateMP4FrameReader")]
        public static extern IntPtr CreateMP4FrameReader ([MarshalAs(UnmanagedType.LPStr)] string url);
        [DllImport(Assembly, EntryPoint = @"NRFrameSize")]
        public static extern void FrameSize (this IntPtr frameReader, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"NRFrameRate")]
        public static extern float FrameRate (this IntPtr frameReader);
        #endregion
    }
}