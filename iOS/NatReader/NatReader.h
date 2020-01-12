//
//  NatReader.h
//  NatReader
//
//  Created by Yusuf Olokoba on 1/12/20.
//  Copyright © 2020 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include "stdint.h"

// Platform defines
#ifdef __cplusplus
    #define BRIDGE extern "C"
#else
    #define BRIDGE
#endif

#ifdef _WIN64
    #define EXPORT __declspec(dllexport)
#else
    #define EXPORT
    #define APIENTRY
#endif


#pragma region --MediaReader--

/*!
 @function NRCreateMP4FrameReader
 
 @abstract Create an MP4 frame reader.
 
 @discussion Create an MP4 frame reader.
 
 @param uri
 URL to media source. MUST be prepended with URI scheme/protocol.
 
 @param startTime
 Media time to start reading samples in seconds.
 
 @param duration
 Duration in seconds.
 */
BRIDGE EXPORT void* APIENTRY NRCreateMP4FrameReader (const char* uri, float startTime, float duration);

/*!
 @function NRMediaURI
 
 @abstract Get the media source URI.
 
 @discussion Get the media source URI.
 
 @param reader
 Opaque pointer to a media reader.
 
 @param dstString
 Destination string.
 */
BRIDGE EXPORT void APIENTRY NRMediaURI (void* reader, char* dstString);

/*!
 @function NRMediaDuration
 
 @abstract Media duration in seconds.
 
 @discussion Media duration in seconds.
 
 @param reader
 Opaque handle to a media reader.
 */
BRIDGE EXPORT float APIENTRY NRMediaDuration (void* reader);

/*!
 @function NRCopyNextFrame
 
 @abstract Copy the next frame from the reader.
 
 @discussion Copy the next frame from the reader.
 
 @param reader
 Opaque handle to a media reader.
 
 @param buffer
 Destination buffer.
 
 @param outBufferSize
 Number of bytes copied in bytes.
 
 @param outTimestamp
 Timestamp of the copied buffer.
 */
BRIDGE EXPORT void APIENTRY NRCopyNextFrame (void* reader, void* buffer, int32_t* outBufferSize, int64_t* outTimestamp);

/*!
 @function NRReset
 
 @abstract Reset the reader to start reading from the initial posiiton.
 
 @discussion Reset the reader to start reading from the initial posiiton.
 
 @param reader
 Opaque handle to a media reader.
 */
BRIDGE EXPORT void APIENTRY NRReset (void* reader);

/*!
 @function NRDispose
 
 @abstract Dispose a media reader.
 
 @discussion Dispose a media reader.
 
 @param reader
 Opaque pointer to a media reader.
 */
BRIDGE EXPORT void APIENTRY NRDispose (void* reader);
#pragma endregion


#pragma region --FrameReader--

/*!
 @function NRFrameSize
 
 @abstract Get the media frame size.
 
 @discussion Get the media frame size.
 
 @param frameReader
 Opaque handle to a frame reader.
 
 @param outWidth
 Frame width.
 
 @param outHeight
 Frame height.
 */
BRIDGE EXPORT void APIENTRY NRFrameSize (void* frameReader, int32_t* outWidth, int32_t* outHeight);

/*!
 @function NRFrameRate
 
 @abstract Get the media frame rate.
 
 @discussion Get the media frame rate.
 
 @param frameReader
 Opaque handle to a frame reader.
 */
BRIDGE EXPORT float APIENTRY NRFrameRate (void* frameReader);
#pragma endregion
