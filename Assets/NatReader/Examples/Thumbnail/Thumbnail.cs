/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System;

    public class Thumbnail : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;
        private FrameReader frameReader;

        void Start () {
            frameReader = new FrameReader(@"");
            frameReader.StartReading(OnFrame);
        }

        void OnFrame (IntPtr pixelBuffer, long timestamp) {
            var frame = new Texture2D(frameReader.pixelWidth, frameReader.pixelHeight, TextureFormat.RGBA32, false, false);
            frame.LoadRawTextureData(pixelBuffer, frame.width * frame.height * 4);
            rawImage.texture = frame;
            aspectFitter.aspectRatio = frame.width / (float)frame.height;
            frameReader.Dispose();
        }
    }
}