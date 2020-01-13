//
// 	Bridge.cpp
// 	NatReader
//
//  Created by Yusuf Olokoba on 11/15/19.
//  Copyright (c) 2020 Yusuf Olokoba. All rights reserved.
//

#include "pch.hpp"
#include "NatReader.h"
#include "IMediaReader.hpp"

void* NRCreateMP4FrameReader (const char* recordingPath, float startTime, float duration) {
	return static_cast<void*>(new MP4FrameReader(recordingPath, startTime, duration));
}

void NRMediaURI (void* readerPtr, char* dstString) {
	IMediaReader* reader = static_cast<IMediaReader*>(readerPtr);
	strcpy(dstString, reader->uri);
}

float NRMediaDuration (void* readerPtr) {
	IMediaReader* reader = static_cast<IMediaReader*>(readerPtr);
	return reader->Duration();
}

void NRCopyNextFrame (void* readerPtr, void* dstBuffer, int32_t* byteSize, int64_t* timestamp) {
	IMediaReader* reader = static_cast<IMediaReader*>(readerPtr);
	reader->CopyNextFrame(dstBuffer, byteSize, timestamp);
}

void NRReset (void* readerPtr) {
	IMediaReader* reader = static_cast<IMediaReader*>(readerPtr);
	reader->Reset();
}

void NRDispose (void* readerPtr) {
	IMediaReader* reader = static_cast<IMediaReader*>(readerPtr);
	delete reader;
}

void NRFrameSize (void* readerPtr, int32_t* width, int32_t* height) {
	MP4FrameReader* reader = static_cast<MP4FrameReader*>(readerPtr);
	*width = reader->FrameWidth();
	*height = reader->FrameHeight();
}

float NRFrameRate (void* readerPtr) {
	MP4FrameReader* reader = static_cast<MP4FrameReader*>(readerPtr);
	return reader->FrameRate();
}
