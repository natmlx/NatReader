//
//  Bridge.m
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

#import "NRFrameExtractor.h"

void* NRCreateFrameExtractor (void (*callback) (void*, uint8_t*, int32_t, int32_t, int64_t), void* context) {
    NRFrameExtractor* extractor = [NRFrameExtractor.alloc initWithFrameBlock:^(uint8_t* pixelBuffer, int32_t width, int32_t height, int64_t timestamp) {
        callback(context, pixelBuffer, width, height, timestamp);
    }];
    return (__bridge_retained void*)extractor;
}

void NRExtract (NRFrameExtractor* extractor, const char* url) {
    [extractor extract:[NSURL URLWithString:[NSString stringWithUTF8String:url]]];
}

void NRDispose (void* extractorPtr) {
    NRFrameExtractor* extractor = (__bridge_transfer NRFrameExtractor*)extractorPtr;
    [extractor dispose];
    extractor = nil;
}
