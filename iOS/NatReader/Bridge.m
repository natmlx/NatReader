//
//  Bridge.m
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

#import "NatReader.h"
#import "NRMediaReader.h"

void* NRCreateFrameReader (const char* url, float startTime, float duration) {
    NSURL* uri = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
    NRMP4FrameReader* reader = [NRMP4FrameReader.alloc initWithURI:uri startTime:startTime andDuration:duration];
    return (__bridge_retained void*)reader;
}

void NRMediaURI (void* readerPtr, char* dstString) {
    id<NRMediaReader> reader = (__bridge id<NRMediaReader>)readerPtr;
}

float NRMediaDuration (void* readerPtr) {
    id<NRMediaReader> reader = (__bridge id<NRMediaReader>)readerPtr;
    return 0;
}

void NRCopyNextFrame (void* readerPtr, void* buffer, int32_t* bufferSize, int64_t* timestamp) {
    id<NRMediaReader> reader = (__bridge id<NRMediaReader>)readerPtr;
    [reader copyNextFrame:buffer withSize:bufferSize andTimestamp:timestamp];
}

void NRReset (void* readerPtr) {
    id<NRMediaReader> reader = (__bridge id<NRMediaReader>)readerPtr;
    [reader reset];
}

void NRDispose (void* readerPtr) {
    id<NRMediaReader> reader = (__bridge_transfer id<NRMediaReader>)readerPtr;
    [reader dispose];
    reader = nil;
}

void NRFrameSize (void* readerPtr, int32_t* width, int32_t* height) {
    NRMP4FrameReader* reader = (__bridge NRMP4FrameReader*)readerPtr;
    *width = reader.frameSize.width;
    *height = reader.frameSize.height;
}

float NRFrameRate (void* readerPtr) {
    NRMP4FrameReader* reader = (__bridge NRMP4FrameReader*)readerPtr;
    return reader.frameRate;
}
