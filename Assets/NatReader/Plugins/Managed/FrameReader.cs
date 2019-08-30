/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader {

    using System;

    public class FrameReader : IDisposable {
        
        public FrameReader (FrameHandler frameHandler) {

        }

        public void StartReading (string url) {
            
        }

        public void Dispose () {

        }
    }

    public delegate void FrameHandler (byte[] pixelBuffer, int width, int height, long timestamp);
}