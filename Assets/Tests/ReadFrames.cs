/* 
*   NatReader
*   Copyright (c) 2020 Yusuf Olokoba.
*/

namespace NatSuite.Tests {

    using System.Threading.Tasks;
    using UnityEngine;
    using Readers;

    public class ReadFrames : MonoBehaviour {

        async void Start () {
            var reader = new MP4FrameReader(Examples.Playback.VideoPath);
            foreach (var (pixelBuffer, timestamp) in reader.Read())
                await Task.Delay(600_000);
        }
    }
}