/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using Readers;

    public class Playback : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        IEnumerator Start () {
            var basePath = Application.platform == RuntimePlatform.Android ? Application.persistentDataPath : Application.streamingAssetsPath;
            var videoPath = "file://" + basePath + "/city.mp4";
            using (var reader = new MP4FrameReader(videoPath)) {
                Debug.Log($"Duration: {reader.duration} Size: {reader.frameSize} Framerate: {reader.frameRate} URI: {reader.uri}");
                // Create and display frame texture
                var frameTexture = new Texture2D(reader.frameSize.width, reader.frameSize.height, TextureFormat.RGBA32, false, false);
                rawImage.texture = frameTexture;
                aspectFitter.aspectRatio = frameTexture.width / (float)frameTexture.height;
                // Render all frames
                foreach (var (pixelBuffer, timestamp) in reader.Read()) {
                    frameTexture.LoadRawTextureData(pixelBuffer);
                    frameTexture.Apply();
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
}