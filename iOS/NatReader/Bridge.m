//
//  Bridge.m
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2020 Yusuf Olokoba. All rights reserved.
//

#import "NatReader.h"
#import "NRMediaReader.h"

void NRDisposeReader (void* readerPtr) {
    id<NRMediaReader> reader = (__bridge_transfer id<NRMediaReader>)readerPtr;
    reader = nil;
}

void NRMediaURI (void* readerPtr, char* dstString) {
    id<NRMediaReader> reader = (__bridge id<NRMediaReader>)readerPtr;
    strcpy(dstString, reader.uri.absoluteString.UTF8String);
}

float NRMediaDuration (void* readerPtr) {
    id<NRMediaReader> reader = (__bridge id<NRMediaReader>)readerPtr;
    return reader.duration;
}

void* NRCreateMP4FrameReader (const char* url) {
    NSURL* uri = [NSURL fileURLWithPath:[NSString stringWithUTF8String:url]];
    NRMP4FrameReader* reader = [NRMP4FrameReader.alloc initWithURI:uri];
    return (__bridge_retained void*)reader;
}

void NRFrameSize (void* frameReaderPtr, int32_t* width, int32_t* height) {
    id<NRFrameReader> reader = (__bridge id<NRFrameReader>)frameReaderPtr;
    *width = reader.frameSize.width;
    *height = reader.frameSize.height;
}

float NRFrameRate (void* frameReaderPtr) {
    id<NRFrameReader> reader = (__bridge id<NRFrameReader>)frameReaderPtr;
    return reader.frameRate;
}

void* NRCreateEnumerator (void* readerPtr, float startTime, float duration) {
    id<NRMediaReader> reader = (__bridge id<NRMediaReader>)readerPtr;
    CMTime startCMTime = CMTimeMakeWithSeconds(startTime, NSEC_PER_SEC);
    CMTime durationCMTime = CMTimeMakeWithSeconds(duration, NSEC_PER_SEC);
    CMTimeRange timeRange = CMTimeRangeMake(startCMTime, durationCMTime);
    id<NRMediaEnumerator> enumerator = [reader createEnumeratorForTimeRange:timeRange];
    return (__bridge_retained void*)enumerator;
}

void NRDisposeEnumerator (void* enumeratorPtr) {
    id<NRMediaEnumerator> enumerator = (__bridge_transfer id<NRMediaEnumerator>)enumeratorPtr;
    [enumerator dispose];
}

void NRCopyNextFrame (void* enumeratorPtr, void* buffer, int32_t* outBufferSize, int64_t* outTimestamp) {
    id<NRMediaEnumerator> enumerator = (__bridge id<NRMediaEnumerator>)enumeratorPtr;
    [enumerator copyNextFrame:buffer withSize:outBufferSize andTimestamp:outTimestamp];
}
