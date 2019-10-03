/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Internal {

    using System;
    using System.Runtime.InteropServices;

    public static class MediaReaderBridge {

        private const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatReader";
        #endif

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"NRCreateFrameReader")]
        public static extern IntPtr CreateFrameReader (string url, long startTime);
        [DllImport(Assembly, EntryPoint = @"NRGetFrameSize")]
        public static extern void GetFrameSize (this IntPtr reader, out int width, out int height);
        [DllImport(Assembly, EntryPoint = @"NRCopyNextFrame")]
        public static extern bool CopyNextFrame (this IntPtr reader, IntPtr dstBuffer, out int byteSize, out long timestamp);
        [DllImport(Assembly, EntryPoint = @"NRDispose")]
        public static extern void Dispose (this IntPtr reader);
        #else
        public static IntPtr CreateFrameReader (string url, long startTime) { return IntPtr.Zero; }
        public static void GetFrameSize (this IntPtr reader, out int width, out int height) { width = height = 0; }
        public static bool CopyNextFrame (this IntPtr reader, IntPtr dstBuffer, out int byteSize, out long timestamp) { byteSize = 0; timestamp = 0; return false; }
        public static void Dispose (this IntPtr reader) {}
        #endif
    }
}