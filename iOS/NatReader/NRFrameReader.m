//
//  NRFrameReader.m
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

@import Accelerate;
@import AVFoundation;
#import "NRMediaReader.h"

@interface NRFrameReader ()
@property AVAssetTrack* videoTrack;
@property AVAssetReader* reader;
@property AVAssetReaderTrackOutput* readerOutput;
@end


@implementation NRFrameReader

@synthesize videoTrack;
@synthesize reader;
@synthesize readerOutput;

- (instancetype) initWithURI:(NSURL*) uri startTime:(float) startTime andDuration:(float) duration { // INCOMPLETE // Negative start time
    self = super.init;
    AVAsset* asset = [AVURLAsset URLAssetWithURL:uri options:nil];
    self.videoTrack = [asset tracksWithMediaType:AVMediaTypeVideo].firstObject;
    NSError* error = nil;
    AVAssetReader* reader = [AVAssetReader.alloc initWithAsset:asset error:&error];
    AVAssetReaderTrackOutput* readerOutput = nil;
    if (!error) {
        readerOutput = [AVAssetReaderTrackOutput.alloc initWithTrack:self.videoTrack outputSettings:@{ (id)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA) }];
        readerOutput.alwaysCopiesSampleData = NO;
        [reader addOutput:readerOutput];
        CMTime rangeStart = CMTimeMakeWithSeconds(startTime, NSEC_PER_SEC);
        CMTime rangeEnd = duration > 0 ? CMTimeMakeWithSeconds(startTime + duration, NSEC_PER_SEC) : kCMTimePositiveInfinity;
        reader.timeRange = CMTimeRangeMake(rangeStart, rangeEnd);
        if (reader.startReading)
            NSLog(@"NatReader: Created FrameReader for media at '%@' with size %fx%f", uri, videoTrack.naturalSize.width, videoTrack.naturalSize.height);
        else
            NSLog(@"NatReader Error: Failed to start reading samples from asset at '%@'", uri);
            
    }
    else
        NSLog(@"NatReader Error: Failed to create asset reader for asset at '%@' with error: %@", uri, error);
    self.reader = reader;
    self.readerOutput = readerOutput;
    return self;
}

- (bool) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp {
    CMSampleBufferRef sampleBuffer = readerOutput.copyNextSampleBuffer;
    if (!sampleBuffer)
        return false;
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
    return true;
}

- (void) dispose {
    [reader cancelReading];
    reader = nil;
}

- (CGSize) frameSize {
    return videoTrack.naturalSize;
}

- (float) frameRate {
    return videoTrack.nominalFrameRate;
}

@end
