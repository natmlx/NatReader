//
//  FrameReader.cpp
//  NatReader
//
//  Created by Yusuf Olokoba on 11/15/19.
//  Copyright (c) 2019 Yusuf Olokoba. All rights reserved.
//

#include "pch.hpp"
#include "IMediaReader.hpp"

FrameReader::FrameReader (const wchar_t* uri, int64_t startTime) { // CHECK // MFStartup?
	// Create attributes
	IMFAttributes* attributes = nullptr;
	MFCreateAttributes(&attributes, 2);
	attributes->SetUINT32(MF_LOW_LATENCY, true);
	attributes->SetUINT32(MF_READWRITE_ENABLE_HARDWARE_TRANSFORMS, true);
	// Create source reader
	MFCreateSourceReaderFromURL(uri, attributes, &frameReader);
	attributes->Release();
	// Inspect video
	IMFMediaType* videoFormat = nullptr;
	uint32_t numerator, denominator;
	frameReader->GetNativeMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, MF_SOURCE_READER_CURRENT_TYPE_INDEX, &videoFormat);
	MFGetAttributeSize(videoFormat, MF_MT_FRAME_SIZE, &pixelWidth, &pixelHeight);
	MFGetAttributeRatio(videoFormat, MF_MT_FRAME_RATE, &numerator, &denominator);
	this->framerate = (float)numerator / (float)denominator;
	videoFormat->Release();
	// Create output format
	IMFMediaType* outputFormat = nullptr;
	MFCreateMediaType(&outputFormat);
	outputFormat->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
	outputFormat->SetGUID(MF_MT_SUBTYPE, MFVideoFormat_RGB32);
	frameReader->SetCurrentMediaType(MF_SOURCE_READER_FIRST_VIDEO_STREAM, nullptr, outputFormat);
	outputFormat->Release();
}

FrameReader::~FrameReader () {
	// Release frame reader
	frameReader->Release();
}

bool FrameReader::CopyNextFrame (void* dstBuffer, uint32_t* outSize, int64_t* outTimestamp) {
	// Read sample
	IMFSample* sample = nullptr;
	DWORD flags = 0;
	frameReader->ReadSample(MF_SOURCE_READER_FIRST_VIDEO_STREAM, 0, nullptr, &flags, &outTimestamp, &sample);
	// Check for EOS
	if ((flags & MF_SOURCE_READERF_ENDOFSTREAM))
		return false;
	// Check empty sample
	if (!sample) {
		*outTimestamp = -1L; // So that client can consider frame to be spurious and skip
		return true;
	}
	// Read pixel buffer
	IMFMediaBuffer* sampleBuffer;
	IMF2DBuffer* pixelBuffer;
    sample->ConvertToContiguousBuffer(&sampleBuffer);
	sampleBuffer->QueryInterface(IID_IMF2DBuffer, (void**)&pixelBuffer);
	pixelBuffer->GetContiguousLength(outSize);
	pixelBuffer->ContiguousCopyTo(dstBuffer, outSize);
	*outTimestamp = *outTimestamp * 100L;
	// Release
	pixelBuffer->Release();
	sampleBuffer->Release();
    sample->Release();
	return true;
}

void FrameReader::GetDimensions (int32_t* width, int32_t* height, float* framerate) {
	*width = (int32_t)this->pixelWidth;
	*height = (int32_t)this->pixelHeight;
	*framerate = this->framerate;
}