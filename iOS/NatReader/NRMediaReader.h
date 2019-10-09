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
@required
- (bool) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp;
- (void) dispose;
@end

@interface NRFrameReader : NSObject <NRMediaReader>
- (instancetype) initWithURI:(NSURL*) uri andStartTime:(int64_t) startTime;
- (bool) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outSize andTimestamp:(int64_t*) outTimestamp;
- (void) dispose;
@property (readonly) CGSize frameSize;
@property (readonly) float frameRate;
@end
