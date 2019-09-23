//
//  NRFrameExtractor.h
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

@import Foundation;
@import CoreVideo;

typedef void (^FrameBlock) (uint8_t* pixelBuffer, int32_t width, int32_t height, int64_t timestamp);

@interface NRFrameExtractor : NSObject
- (instancetype) initWithFrameBlock:(FrameBlock) frameBlock;
- (void) extract:(NSURL*) url;
- (void) dispose;
@end
