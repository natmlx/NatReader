/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using System.IO;
    using Readers;

    public class Playback : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        IEnumerator Start () {
            using (var reader = new MP4FrameReader(VideoPath)) {
                Debug.Log($"Frame: {reader.frameSize} @{reader.frameRate}Hz Duration: {reader.duration} URI: {reader.uri}");
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

        private static string VideoPath {
            get {
                const string Name = "city.mp4";
                var path = string.Empty;
                switch (Application.platform) {
                    case RuntimePlatform.Android: path = Path.Combine(Application.persistentDataPath, Name); break;
                    case RuntimePlatform.IPhonePlayer: path = Path.Combine(Application.streamingAssetsPath, Name); break;
                    case RuntimePlatform.OSXEditor: goto case RuntimePlatform.WindowsEditor;
                    case RuntimePlatform.WindowsEditor: path = Path.Combine(Directory.GetCurrentDirectory(), $"Assets/StreamingAssets/{Name}"); break;
                    default: return "";
                }
                return "file://" + path;
            }
        }
    }
}