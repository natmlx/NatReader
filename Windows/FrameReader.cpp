//
//  FrameReader.cpp
//  NatReader
//
//  Created by Yusuf Olokoba on 11/15/19.
//  Copyright (c) 2019 Yusuf Olokoba. All rights reserved.
//

#include "pch.hpp"
#include "IMediaReader.hpp"

FrameReader::FrameReader (const wchar_t* uri, int64_t startTime) {

}

FrameReader::~FrameReader () {

}

bool FrameReader::CopyNextFrame (void* dstBuffer, int32_t* outSize, int64_t* outTimestamp) {
	return false;
}

void FrameReader::GetDimensions (int32_t* width, int32_t* height, float* framerate) {

}