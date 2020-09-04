/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Tests {

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Readers;

    public class StepFrames : MonoBehaviour {

        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        MP4Reader reader;
        IEnumerator<(byte[], long)> enumerator;
        Texture2D frameTexture;

        void Start () {
            // Create reader
            reader = new MP4Reader(Examples.Playback.VideoPath);
            enumerator = reader.Read().GetEnumerator();
            // Create frame texture
            frameTexture = new Texture2D(reader.frameSize.width, reader.frameSize.height, TextureFormat.RGBA32, false, false);
            rawImage.texture = frameTexture;
            aspectFitter.aspectRatio = frameTexture.width / (float)frameTexture.height;
        }

        void Update () {
            if (Input.touchCount > 0) {
                var touch = Input.GetTouch(0);
                if (touch.tapCount == 2 && touch.phase == TouchPhase.Ended) {
                    // Move
                    enumerator.MoveNext();
                    // Render
                    var (pixelBuffer, timestamp) = enumerator.Current;
                    frameTexture.LoadRawTextureData(pixelBuffer);
                    frameTexture.Apply();
                    Debug.Log($"Rendered frame with timestamp: {timestamp / 1e+9f}");
                }
            }
        }
    }
}