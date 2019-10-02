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
@property FrameBlock frameBlock;
@property AVAssetTrack* videoTrack;
@property AVAssetReader* reader;
@property AVAssetReaderTrackOutput* readerOutput;
@end


@implementation NRFrameReader

- (instancetype) initWithURI:(NSURL*) uri {
    self = super.init;
    AVAsset* asset = [AVURLAsset URLAssetWithURL:uri options:nil];
    self.videoTrack = [asset tracksWithMediaType:AVMediaTypeVideo].firstObject;
    NSError* error = nil;
    self.reader = [AVAssetReader.alloc initWithAsset:asset error:&error];
    if (!error) {
        self.readerOutput = [AVAssetReaderTrackOutput.alloc initWithTrack:self.videoTrack outputSettings:@{ (id)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA) }];
        self.readerOutput.alwaysCopiesSampleData = NO;
        [self.reader addOutput:self.readerOutput];
    }
    else {
        NSLog(@"NatReader Error: Failed to create asset reader for asset at %@ with error: %@", uri, error);
        self.reader = nil;
    }
    return self;
}

- (void) startReading:(FrameBlock) frameBlock {
    if (self.reader.startReading)
        dispatch_async(
            dispatch_queue_create("NatReader", DISPATCH_QUEUE_SERIAL),
            ^{
                uint8_t* pixelBuffer = NULL;
                for (;;) {
                    CMSampleBufferRef sampleBuffer = self.readerOutput.copyNextSampleBuffer;
                    if (!sampleBuffer)
                        break;
                    CVPixelBufferRef sourceBuffer = CMSampleBufferGetImageBuffer(sampleBuffer);
                    int64_t timestamp = (int64_t)(CMTimeGetSeconds(CMSampleBufferGetPresentationTimeStamp(sampleBuffer)) * 1e+9);
                    CVPixelBufferLockBaseAddress(sourceBuffer, kCVPixelBufferLock_ReadOnly);
                    int width = (int)CVPixelBufferGetWidth(sourceBuffer);
                    int height = (int)CVPixelBufferGetHeight(sourceBuffer);
                    int stride = (int)CVPixelBufferGetBytesPerRow(sourceBuffer);
                    void* baseAddress = CVPixelBufferGetBaseAddress(sourceBuffer);
                    pixelBuffer = pixelBuffer ? pixelBuffer : malloc(width * height * 4);
                    vImage_Buffer input = { baseAddress + stride * (height - 1), height, width, -stride };
                    vImage_Buffer output = { pixelBuffer, height, width, width * 4 };
                    vImagePermuteChannels_ARGB8888(&input, &output, (uint8_t[4]){ 2, 1, 0, 3 }, kvImageNoFlags);
                    CVPixelBufferUnlockBaseAddress(sourceBuffer, kCVPixelBufferLock_ReadOnly);
                    CFRelease(sampleBuffer);
                    dispatch_sync(dispatch_get_main_queue(), ^{ frameBlock(pixelBuffer, timestamp); });
                   
                }
                free(pixelBuffer);
            }
        );
}

- (void) dispose {
    [self.reader cancelReading];
    self.reader = nil;
}

- (CGSize) frameSize {
    return self.videoTrack.naturalSize;
}

@end
