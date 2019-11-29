//
// 	Bridge.cpp
// 	NatReader
//
//  Created by Yusuf Olokoba on 11/15/19.
//  Copyright (c) 2019 Yusuf Olokoba. All rights reserved.
//

#include "pch.hpp"
#include "IMediaReader.hpp"

#define BRIDGE extern "C" __declspec(dllexport)

BRIDGE void* APIENTRY NRCreateFrameReader (const wchar_t* recordingPath, int64_t startTime) {
	return static_cast<void*>(new FrameReader(recordingPath, startTime));
}

BRIDGE void APIENTRY NRDispose (IMediaReader* reader) {
	delete reader;
}

BRIDGE bool APIENTRY NRCopyNextFrame (IMediaReader* reader, void* dstBuffer, uint32_t* byteSize, int64_t* timestamp) {
	return reader->CopyNextFrame(dstBuffer, byteSize, timestamp);
}

BRIDGE void APIENTRY NRFrameReaderGetProperties (FrameReader* reader, int32_t* width, int32_t* height, float* framerate) {
	reader->GetDimensions(width, height, framerate);
}