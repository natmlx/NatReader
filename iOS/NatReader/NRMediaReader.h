//
//  NRMediaReader.h
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2020 Yusuf Olokoba. All rights reserved.
//

@import Foundation;
@import CoreVideo;

@protocol NRMediaReader <NSObject>
@property (readonly) NSURL* uri;
@property (readonly) float duration;
- (void) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp;
- (void) reset;
- (void) dispose;
@end

@interface NRMP4FrameReader : NSObject <NRMediaReader>
- (instancetype) initWithURI:(NSURL*) uri startTime:(float) startTime andDuration:(float) duration;
- (void) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp;
- (void) reset;
- (void) dispose;
@property (readonly) NSURL* uri;
@property (readonly) float duration;
@property (readonly) CGSize frameSize;
@property (readonly) float frameRate;
@end
