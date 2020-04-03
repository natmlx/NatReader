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
@property AVURLAsset* asset;
@property AVAssetTrack* videoTrack;
@end

@interface NRAVAssetReaderEnumerator : NSObject <NRMediaEnumerator>
@property AVAssetReader* reader;
@property AVAssetReaderTrackOutput* readerOutput;
+ (instancetype) enumeratorWithAsset:(AVAsset*) asset track:(AVAssetTrack*) track andTimeRange:(CMTimeRange) timeRange;
@end


@implementation NRMP4FrameReader

@synthesize asset;
@synthesize videoTrack;

- (instancetype) initWithURI:(NSURL*) uri {
    self = super.init;
    self.asset = [AVURLAsset URLAssetWithURL:uri options:nil];
    self.videoTrack = [asset tracksWithMediaType:AVMediaTypeVideo].firstObject;
    if (!videoTrack)
        NSLog(@"NatReader Error: MP4FrameReader failed to find video track for asset at '%@'", uri);
    return self;
}

- (NSURL*) uri {
    return asset.URL;
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

- (id<NRMediaEnumerator>) createEnumeratorForTimeRange:(CMTimeRange) timeRange {
    return asset.readable && videoTrack ? [NRAVAssetReaderEnumerator enumeratorWithAsset:asset track:videoTrack andTimeRange:timeRange] : nil;
}

@end


@implementation NRAVAssetReaderEnumerator

+ (instancetype) enumeratorWithAsset:(AVAsset*) asset track:(AVAssetTrack*) track andTimeRange:(CMTimeRange) timeRange {
    NSError* error;
    AVAssetReader* reader = [AVAssetReader.alloc initWithAsset:asset error:&error];
    if (error) {
        NSLog(@"NatReader Error: Failed to create asset reader for asset '%@' with error: %@", asset, error);
        return nil;
    }
    AVAssetReaderTrackOutput* readerOutput = [AVAssetReaderTrackOutput.alloc initWithTrack:track outputSettings:@{ (id)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA) }];
    readerOutput.alwaysCopiesSampleData = NO;
    readerOutput.supportsRandomAccess = NO;
    [reader addOutput:readerOutput];
    reader.timeRange = timeRange;
    if (!reader.startReading) {
        NSLog(@"NatReader Error: Failed to start reading samples from asset '%@'", asset);
        return nil;
    }
    NRAVAssetReaderEnumerator* enumerator = NRAVAssetReaderEnumerator.alloc.init;
    enumerator.reader = reader;
    enumerator.readerOutput = readerOutput;
    return enumerator;
}

- (void) dispose {
    [self.reader cancelReading];
}

- (void) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outBufferSize andTimestamp:(int64_t*) outTimestamp {
    CMSampleBufferRef sampleBuffer = self.readerOutput.copyNextSampleBuffer;
    if (!sampleBuffer) {
        *outBufferSize = 0;
        *outTimestamp = -1L;
        return;
    }
    CVPixelBufferRef sourceBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
    CMTime bufferTimestamp = CMSampleBufferGetPresentationTimeStamp(sampleBuffer);
    CVPixelBufferLockBaseAddress(sourceBuffer, kCVPixelBufferLock_ReadOnly);
    int width = (int)CVPixelBufferGetWidth(sourceBuffer);
    int height = (int)CVPixelBufferGetHeight(sourceBuffer);
    int stride = (int)CVPixelBufferGetBytesPerRow(sourceBuffer);
    void* baseAddress = CVPixelBufferGetBaseAddress(sourceBuffer);
    vImage_Buffer input = { baseAddress + stride * (height - 1), height, width, -stride };
    vImage_Buffer output = { dstBuffer, height, width, width * 4 };
    vImagePermuteChannels_ARGB8888(&input, &output, (uint8_t[4]){ 2, 1, 0, 3 }, kvImageNoFlags);
    CVPixelBufferUnlockBaseAddress(sourceBuffer, kCVPixelBufferLock_ReadOnly);
    CFRelease(sampleBuffer);
    *outBufferSize = width * height * 4;
    *outTimestamp = (int64_t)(CMTimeGetSeconds(bufferTimestamp) * 1e+9);
}

@end
