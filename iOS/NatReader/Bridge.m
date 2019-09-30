//
//  Bridge.m
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

#import "NRMediaReader.h"

void* NRCreateFrameExtractor (const char* url) {
    NSURL* uri = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
    NRFrameReader* reader = [NRFrameReader.alloc initWithURI:uri];
    return (__bridge_retained void*)reader;
}

void NRStartReading (id<NRMediaReader> reader, void (*callback) (void*, uint8_t*, int64_t), void* context) {
    [reader startReading:^(uint8_t *pixelBuffer, int64_t timestamp) {
        callback(context, pixelBuffer, timestamp);
    }];
}

void NRDispose (void* readerPtr) {
    id<NRMediaReader> reader = (__bridge_transfer id<NRMediaReader>)readerPtr;
    [reader dispose];
    reader = nil;
}

int NRPixelWidth (id<NRMediaReader> reader) {
    return reader.frameSize.width;
}

int NRPixelHeight (id<NRMediaReader> reader) {
    return reader.frameSize.height;
}
