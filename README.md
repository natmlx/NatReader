# NatReader API
NatReader is a lightweight video decoding API designed for transcoding applications. It currently supports decoding frames from local video files (`*.mp4`).

## Setup Instructions
NatReader can be installed using the Unity Package Manager. In your `manifest.json` file, add the following dependency:
```json
{
  "dependencies": {
    "com.natsuite.natreader": "git+https://github.com/natsuite/NatReader"
  }
}
```

## Decoding Video Frames
First, create a frame reader for your media file. Currently, NatReader only supports MP4 video files:
```csharp
var videoPath = "...";
var reader = new MP4Reader(videoPath);
```
With the reader, you can decode frames using the `Read` method:
```csharp
foreach (var (pixelBuffer, timestamp) in reader.Read()) {
    // Use pixel buffer // This is always a `byte[]` in RGBA32 layout
    ...
}
```
Finally, when you are done reading frames, make sure to dispose the reader:
```csharp
reader.Dispose();
```

## Requirements
- Unity 2019.2+
- Android API Level 24+
- iOS 11+
- macOS 10.13+