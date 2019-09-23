//
//  NRFrameExtractor.m
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

@import Accelerate;
@import AVFoundation;
#import "NRFrameExtractor.h"

@interface NRFrameExtractor ()
@property FrameBlock frameBlock;
@property AVAssetReader* reader;
@end


@implementation NRFrameExtractor

- (instancetype) initWithFrameBlock:(FrameBlock) frameBlock {
    self = super.init;
    self.frameBlock = frameBlock;
    return self;
}

- (void) extract:(NSURL*) url {
    AVAsset* asset = [AVURLAsset URLAssetWithURL:url options:nil];
    AVAssetTrack* videoTrack = [asset tracksWithMediaType:AVMediaTypeVideo].firstObject;
    NSError* error = nil;
    self.reader = [AVAssetReader.alloc initWithAsset:asset error:&error];
    if (error) {
        NSLog(@"NatReader Error: Failed to create asset reader for asset at %@ with error: %@", url, error);
        return;
    }
    id options = @{ (id)kCVPixelBufferPixelFormatTypeKey: @(kCVPixelFormatType_32BGRA) };
    AVAssetReaderTrackOutput* readerOutput = [AVAssetReaderTrackOutput.alloc initWithTrack:videoTrack outputSettings:options];
    readerOutput.alwaysCopiesSampleData = NO;
    [self.reader addOutput:readerOutput];
    [self.reader startReading];
    dispatch_async(
        dispatch_queue_create("NatReader", DISPATCH_QUEUE_SERIAL),
        ^{
            uint8_t* pixelBuffer = NULL;
            for (;;) {
                CMSampleBufferRef sampleBuffer = readerOutput.copyNextSampleBuffer;
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
                vImage_Buffer input = { baseAddress, height, width, stride };
                vImage_Buffer output = { pixelBuffer, height, width, width * 4 };
                vImageVerticalReflect_ARGB8888(&input, &output, kvImageNoFlags); // CHECK // If swizzle, then use inversion hack
                CVPixelBufferUnlockBaseAddress(sourceBuffer, kCVPixelBufferLock_ReadOnly);
                self.frameBlock(pixelBuffer, width, height, timestamp);
                CFRelease(sampleBuffer);
            }
            free(pixelBuffer);
        }
    );
}

- (void) dispose {
    [self.reader cancelReading];
    self.reader = nil;
}

@end
