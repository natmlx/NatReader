//
//  NRMP4FrameReader.m
//  NatReader
//
//  Created by Yusuf Olokoba on 10/3/19.
//  Copyright © 2020 Yusuf Olokoba. All rights reserved.
//

@import Accelerate;
@import AVFoundation;
#import "NRMediaReader.h"

@interface NRMP4FrameReader ()
@property NSURL* uri;
@property CMTimeRange timeRange;
@property AVAsset* asset;
@property AVAssetTrack* videoTrack;
@property AVAssetReader* reader;
@property AVAssetReaderTrackOutput* readerOutput;
@end


@implementation NRMP4FrameReader

@synthesize uri;
@synthesize timeRange;
@synthesize asset;
@synthesize videoTrack;
@synthesize reader;
@synthesize readerOutput;

- (instancetype) initWithURI:(NSURL*) uri startTime:(float) startTime andDuration:(float) duration {
    NSError* error = nil;
    self = super.init;
    self.uri = uri;
    self.timeRange = CMTimeRangeMake(CMTimeMakeWithSeconds(startTime, NSEC_PER_SEC), CMTimeMakeWithSeconds(duration, NSEC_PER_SEC));
    self.asset = [AVURLAsset URLAssetWithURL:uri options:nil];
    self.videoTrack = [asset tracksWithMediaType:AVMediaTypeVideo].firstObject;
    self.reader = [AVAssetReader.alloc initWithAsset:asset error:&error];
    if (!error) {
        self.readerOutput = [AVAssetReaderTrackOutput.alloc initWithTrack:videoTrack outputSettings:@{ (id)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA) }];
        readerOutput.alwaysCopiesSampleData = NO;
        readerOutput.supportsRandomAccess = YES;
        [reader addOutput:readerOutput];
        reader.timeRange = timeRange;
        if (reader.startReading)
            NSLog(@"NatReader: Created MP4FrameReader for media at '%@' with size %fx%f", uri, videoTrack.naturalSize.width, videoTrack.naturalSize.height);
        else
            NSLog(@"NatReader Error: MP4FrameReader failed to start reading samples from asset at '%@'", uri);
    }
    else
        NSLog(@"NatReader Error: MP4FrameReader failed to create asset reader for asset at '%@' with error: %@", uri, error);
    return self;
}

- (float) duration {
    return CMTimeGetSeconds(asset.duration);
}

- (CGSize) frameSize {
    return videoTrack.naturalSize;
}

- (float) frameRate {
    return videoTrack.nominalFrameRate;
}

- (void) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp {
    CMSampleBufferRef sampleBuffer = readerOutput.copyNextSampleBuffer;
    if (!sampleBuffer) {
        *outSize = 0;
        *outTimestamp = -1L;
        return;
    }
    CVPixelBufferRef sourceBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
    *outTimestamp = (int64_t)(CMTimeGetSeconds(CMSampleBufferGetPresentationTimeStamp(sampleBuffer)) * 1e+9);
    CVPixelBufferLockBaseAddress(sourceBuffer, kCVPixelBufferLock_ReadOnly);
    int width = (int)CVPixelBufferGetWidth(sourceBuffer);
    int height = (int)CVPixelBufferGetHeight(sourceBuffer);
    int stride = (int)CVPixelBufferGetBytesPerRow(sourceBuffer);
    *outSize = width * height * 4;
    void* baseAddress = CVPixelBufferGetBaseAddress(sourceBuffer);
    vImage_Buffer input = { baseAddress + stride * (height - 1), height, width, -stride };
    vImage_Buffer output = { dstBuffer, height, width, width * 4 };
    vImagePermuteChannels_ARGB8888(&input, &output, (uint8_t[4]){ 2, 1, 0, 3 }, kvImageNoFlags);
    CVPixelBufferUnlockBaseAddress(sourceBuffer, kCVPixelBufferLock_ReadOnly);
    CFRelease(sampleBuffer);
}

- (void) reset {
    [readerOutput resetForReadingTimeRanges:@[[NSValue valueWithCMTimeRange:timeRange]]];
}

- (void) dispose {
    [readerOutput markConfigurationAsFinal];
    [reader cancelReading];
    reader = nil;
}

@end
