/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Examples {

    using UnityEngine;
    using UnityEngine.UI;

    public class Thumbnail : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        void Start () {
            using (var reader = new FrameReader("file://" + Application.streamingAssetsPath + "/city.mp4"))
                foreach (var (pixelBuffer, timestamp) in reader) {
                    var frame = new Texture2D(reader.pixelWidth, reader.pixelHeight, TextureFormat.RGBA32, false, false);
                    frame.LoadRawTextureData(pixelBuffer);
                    frame.Apply();
                    rawImage.texture = frame;
                    aspectFitter.aspectRatio = frame.width / (float)frame.height;
                    break;
                }
        }
    }
}