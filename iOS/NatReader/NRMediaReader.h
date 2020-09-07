//
//  NRMediaReader.h
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2020 Yusuf Olokoba. All rights reserved.
//

@import Foundation;
@import CoreMedia;
@import CoreVideo;

@protocol NRMediaEnumerator;

@protocol NRMediaReader <NSObject>
@property (readonly) NSURL* uri;
@property (readonly) float duration;
- (id<NRMediaEnumerator>) createEnumeratorForTimeRange:(CMTimeRange) timeRange withFrameSkip:(int) frameSkip;
@end

@protocol NRFrameReader <NRMediaReader>
@property (readonly) CGSize frameSize;
@property (readonly) float frameRate;
@end

@protocol NRMediaEnumerator <NSObject>
- (void) copyNextFrame:(void*) dstBuffer withSize:(int32_t*) outBufferSize andTimestamp:(int64_t*) outTimestamp;
- (void) dispose;
@end

@interface NRMP4Reader : NSObject <NRFrameReader>
- (instancetype) initWithURI:(NSURL*) uri;
@end
