/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Tests {

    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using Readers;

    public class ReadFrames : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        IEnumerator Start () {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            // Create reader
            var reader = new MP4Reader(Examples.Playback.VideoPath);
            // Create frame texture
            var frameTexture = new Texture2D(reader.frameSize.width, reader.frameSize.height, TextureFormat.RGBA32, false, false);
            rawImage.texture = frameTexture;
            aspectFitter.aspectRatio = frameTexture.width / (float)frameTexture.height;
            // Render all frames
            var frameCount = 0;
            foreach (var (pixelBuffer, timestamp) in reader.Read()) {
                frameTexture.LoadRawTextureData(pixelBuffer);
                frameTexture.Apply();
                frameCount++;
                Debug.Log($"Rendered frame for time {timestamp / 1e+9f}");
                yield return new WaitForEndOfFrame();
            }
            Debug.Log($"Finished reading {frameCount} frames");
        }
    }
}