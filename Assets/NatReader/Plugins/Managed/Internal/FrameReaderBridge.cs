/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatPlayer.Internal {

    using System;
    using System.Runtime.InteropServices;

    public static class FrameExtractorBridge {

        private const string Assembly =
        #if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        @"__Internal";
        #else
        @"NatReader";
        #endif

        public delegate void FrameHandler (IntPtr context, IntPtr pixelBuffer, int width, int height, int stride, long timestamp);

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"NRCreateFrameExtractor")]
        public static extern IntPtr CreateFrameExtractor (FrameHandler frameHandler, IntPtr context);
        [DllImport(Assembly, EntryPoint = @"NRStartReading")]
        public static extern void StartReading (this IntPtr reader, string url);
        [DllImport(Assembly, EntryPoint = @"NRDispose")]
        public static extern void Dispose (this IntPtr extractor);
        #else
        public static IntPtr CreateFrameExtractor (FrameHandler frameHandler, IntPtr context) { return IntPtr.Zero; }
        public static void StartReading (this IntPtr reader, string url) {}
        public static void Dispose (this IntPtr reader) {}
        #endif
    }
}