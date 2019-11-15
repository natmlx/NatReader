//
//  IMediaReader.hpp
//  NatReader
//
//  Created by Yusuf Olokoba on 11/15/19.
//  Copyright (c) 2019 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <string>

class IMediaReader {
	public:
		virtual ~IMediaReader () {};
		virtual bool CopyNextFrame (void* dstBuffer, int32_t* outSize, int64_t* outTimestamp) = 0;
};

class FrameReader : public IMediaReader {
	public:
		FrameReader (const wchar_t* uri, int64_t startTime);
		~FrameReader ();
		bool CopyNextFrame (void* dstBuffer, int32_t* outSize, int64_t* outTimestamp) override;
		void GetDimensions (int32_t* width, int32_t* height, float* framerate);
	private:
};