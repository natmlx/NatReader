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
 @function NRDispose
 
 @abstract Dispose a media reader.
 
 @discussion Dispose a media reader.
 
 @param reader
 Opaque pointer to a media reader.
 */
BRIDGE EXPORT void APIENTRY NRDisposeReader (void* reader);

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
#pragma endregion


#pragma region --FrameReader--
/*!
 @function NRCreateMP4FrameReader
 
 @abstract Create an MP4 frame reader.
 
 @discussion Create an MP4 frame reader.
 
 @param uri
 URL to media source. MUST be prepended with URI scheme/protocol.
 */
BRIDGE EXPORT void* APIENTRY NRCreateMP4FrameReader (const char* uri);

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


#pragma region --MediaEnumerator--
/*!
 @function NRCreateEnumerator
 
 @abstract Create a media frame enumerator to read frames from the reader.
 
 @discussion Create a media frame enumerator to read frames from the reader.
 
 @param startTime
 Media time to start reading samples in seconds.
    
 @param duration
 Duration in seconds.
 */
BRIDGE EXPORT void* APIENTRY NRCreateEnumerator (void* reader, float startTime, float duration);

/*!
 @function NRDisposeEnumerator
 
 @abstract Dispose a media enumerator.
 
 @discussion Dispose a media enumerator.
 
 @param enumerator
 Opaque handle to a media enumerator.
 */
BRIDGE EXPORT void APIENTRY NRDisposeEnumerator (void* enumerator);

/*!
 @function NRCopyNextFrame
 
 @abstract Copy the next frame from the reader.
 
 @discussion Copy the next frame from the reader.
 
 @param enumerator
 Opaque handle to a media enumeratorr.
 
 @param dstBuffer
 Destination buffer.
 
 @param outBufferSize
 Number of bytes copied in bytes.
 
 @param outTimestamp
 Timestamp of the copied buffer.
 */
BRIDGE EXPORT void APIENTRY NRCopyNextFrame (void* enumerator, void* dstBuffer, int32_t* outBufferSize, int64_t* outTimestamp);
#pragma endregion
