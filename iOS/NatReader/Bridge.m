//
//  Bridge.m
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

#import "NRMediaReader.h"

void* NRCreateFrameReader (const char* url, int64_t startTime) {
    NSURL* uri = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
    NRFrameReader* reader = [NRFrameReader.alloc initWithURI:uri andStartTime:startTime];
    return (__bridge_retained void*)reader;
}

void NRGetFrameSize (NRFrameReader* reader, int32_t* width, int32_t* height) {
    *width = reader.frameSize.width;
    *height = reader.frameSize.height;
}

bool NRCopyNextFrame (id<NRMediaReader> reader, void* dstBuffer, int32_t* byteSize, int64_t* timestamp) {
    return [reader copyNextFrame:dstBuffer withSize:byteSize andTimestamp:timestamp];
}

void NRDispose (void* readerPtr) {
    id<NRMediaReader> reader = (__bridge_transfer id<NRMediaReader>)readerPtr;
    [reader dispose];
    reader = nil;
}
