/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;

    public class Playback : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        IEnumerator Start () {
            using (var reader = new FrameReader("file://" + Application.streamingAssetsPath + "/city.mp4")) {
                // Create and display frame texture
                var frameTexture = new Texture2D(reader.pixelWidth, reader.pixelHeight, TextureFormat.RGBA32, false, false);
                rawImage.texture = frameTexture;
                aspectFitter.aspectRatio = frameTexture.width / (float)frameTexture.height;
                // Render all frames
                foreach (var (pixelBuffer, timestamp) in reader) {
                    frameTexture.LoadRawTextureData(pixelBuffer);
                    frameTexture.Apply();
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
}