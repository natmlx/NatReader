/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Tests {

    using UnityEngine;
    using Readers;

    public class FrameCount : MonoBehaviour {

        void Start () {
            using (var reader = new MP4Reader(Examples.Playback.VideoPath)) {
                var frameCount = 0;
                foreach (var (pixelBuffer, timestamp) in reader.Read())
                    frameCount++;
                Debug.Log($"Video has {frameCount} frames");
            }
        }
    }
}