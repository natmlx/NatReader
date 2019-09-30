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

        public delegate void FrameHandler (IntPtr context, IntPtr pixelBuffer, long timestamp);

        #if UNITY_IOS && !UNITY_EDITOR
        [DllImport(Assembly, EntryPoint = @"NRCreateFrameReader")]
        public static extern IntPtr CreateFrameReader (string url);
        [DllImport(Assembly, EntryPoint = @"NRStartReading")]
        public static extern void StartReading (this IntPtr reader, FrameHandler frameHandler, IntPtr context);
        [DllImport(Assembly, EntryPoint = @"NRDispose")]
        public static extern void Dispose (this IntPtr reader);
        [DllImport(Assembly, EntryPoint = @"NRPixelWidth")]
        public static extern int PixelWidth (this IntPtr reader);
        [DllImport(Assembly, EntryPoint = @"NRPixelHeight")]
        public static extern int PixelHeight (this IntPtr reader);
        #else
        public static IntPtr CreateFrameReader (string url) { return IntPtr.Zero; }
        public static void StartReading (this IntPtr reader, FrameHandler frameHandler, IntPtr context) {}
        public static void Dispose (this IntPtr reader) {}
        public static int PixelWidth (this IntPtr reader) { return 0; }
        public static int PixelHeight (this IntPtr reader) { return 0; }
        #endif
    }
}