# NatReader API
NatReader is a lightweight video decoding API designed for transcoding applications. It currently supports decoding frames from local video files (`*.mp4`).

## Usage
Simply create a frame reader then iterate through the frames within it:
```csharp
var videoPath = "file:///path/to/some/video.mp4";
using (var reader = new MP4FrameReader(videoPath))
    foreach (var (pixelBuffer, timestamp) in reader.Read()) {
        // `pixelBuffer` is a `byte[]` with the frame pixel data in RGBA32 layout
        // `timestamp` is the frame timestamp in nanoseconds
    }
```

## Requirements
- Unity 2018.3+
- Android API Level 23+
- iOS 11+
- macOS 10.13+

## Notes
- On Android, `MP4Reader` may support decoding frames from a remote URL.

Thank you!