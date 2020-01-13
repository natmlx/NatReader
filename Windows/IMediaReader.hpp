//
//  IMediaReader.hpp
//  NatReader
//
//  Created by Yusuf Olokoba on 11/15/19.
//  Copyright (c) 2020 Yusuf Olokoba. All rights reserved.
//

#pragma once

#include <mfapi.h>
#include <mfidl.h>
#include <mfreadwrite.h>
#include <string>

#pragma comment(lib, "mf")
#pragma comment(lib, "mfplat")
#pragma comment(lib, "mfreadwrite")
#pragma comment(lib, "mfuuid")

class IMediaReader {
	public:
		virtual ~IMediaReader () { }
		virtual float Duration () = 0;
		virtual void CopyNextFrame (void* dstBuffer, int32_t* outSize, int64_t* outTimestamp) = 0;
		virtual void Reset () = 0;
		const std::string uri;
};

class MP4FrameReader : public IMediaReader {
	public:
		FrameReader (const wchar_t* uri, float startTime, float duration);
		~FrameReader ();
		void Duration () override;
		void CopyNextFrame (void* dstBuffer, int32_t* outSize, int64_t* outTimestamp) override;
		void Reset () override;
		int32_t FrameWidth () const;
		int32_t FrameHeight () const;
		float FrameRate () const;
	private:
		static bool initializedMF;
		IMFSourceReader* frameReader;
		uint32_t pixelWidth, pixelHeight, rowStride;
		float framerate;
		int64_t endTimestamp;
};