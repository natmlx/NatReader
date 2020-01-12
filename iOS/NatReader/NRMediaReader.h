//
//  NRMediaReader.h
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

@import Foundation;
@import CoreVideo;

@protocol NRMediaReader <NSObject>
@property (readonly) NSString* uri;
@property (readonly) float duration; // INCOMPLETE // Check type
- (bool) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp;
- (void) reset;
- (void) dispose;
@end

@interface NRMP4FrameReader : NSObject <NRMediaReader>
@property (readonly) NSString* uri;
@property (readonly) float duration; // INCOMPLETE // Check type
- (instancetype) initWithURI:(NSURL*) uri startTime:(float) startTime andDuration:(float) duration;
- (bool) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp;
- (void) reset;
- (void) dispose;
@property (readonly) CGSize frameSize;
@property (readonly) float frameRate;
@end
