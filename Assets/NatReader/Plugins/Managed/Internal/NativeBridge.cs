/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatReader.Internal {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class NativeBridge {

        private const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatReader";
        #endif

        [DllImport(Assembly, EntryPoint = @"NRCreateMP4FrameReader")]
        public static extern IntPtr CreateMP4FrameReader ([MarshalAs(UnmanagedType.LPStr)] string url, float startTime, float duration);
        [DllImport(Assembly, EntryPoint = @"NRMediaURI")]
        public static extern void URI (this IntPtr reader, [MarshalAs(UnmanagedType.LPStr)] StringBuilder dstString);
        [DllImport(Assembly, EntryPoint = @"NRMediaDuration")]
        public static extern float Duration (this IntPtr reader);
        [DllImport(Assembly, EntryPoint = @"NRCopyNextFrame")]
        public static extern void CopyNextFrame (this IntPtr reader, IntPtr buffer, out int bufferSize, out long timestamp);
        [DllImport(Assembly, EntryPoint = @"NRReset")]
        public static extern void Reset (this IntPtr reader);
        [DllImport(Assembly, EntryPoint = @"NRDispose")]
        public static extern void Dispose (this IntPtr reader);

        [DllImport(Assembly, EntryPoint = @"NRFrameSize")]
        public static extern void FrameSize (this IntPtr frameReader, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"NRFrameRate")]
        public static extern float FrameRate (this IntPtr frameReader);
    }
}