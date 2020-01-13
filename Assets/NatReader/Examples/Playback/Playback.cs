/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatReader.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;

    public class Playback : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        IEnumerator Start () {
            var basePath = Application.platform == RuntimePlatform.Android ? Application.persistentDataPath : Application.streamingAssetsPath;
            using (var reader = new MP4FrameReader("file://" + basePath + "/city.mp4")) {
                Debug.Log($"Duration: {reader.duration} Size: {reader.frameSize} Framerate: {reader.frameRate} URI: {reader.uri}");
                // Create and display frame texture
                var frameTexture = new Texture2D(reader.frameSize.width, reader.frameSize.height, TextureFormat.RGBA32, false, false);
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