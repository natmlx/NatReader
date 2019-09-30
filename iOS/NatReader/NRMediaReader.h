//
//  NRMediaReader.h
//  NatReader
//
//  Created by Yusuf Olokoba on 9/23/19.
//  Copyright © 2019 Yusuf Olokoba. All rights reserved.
//

@import Foundation;
@import CoreVideo;

typedef void (^FrameBlock) (uint8_t* pixelBuffer, int64_t timestamp);

@protocol NRMediaReader <NSObject>
@required
- (void) startReading:(FrameBlock) frameBlock;
- (void) dispose;
@property (readonly) CGSize frameSize;
@end

@interface NRFrameReader : NSObject <NRMediaReader>
- (instancetype) initWithURI:(NSURL*) uri;
- (void) startReading:(FrameBlock) frameBlock;
- (void) dispose;
@property (readonly) CGSize frameSize;
@end
