/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

const NatReaderWebGL = {

    $sharedInstance : {

    },

    NRCreateFrameReader : function (uri, startTime) {
        var videoElement = document.createElement("video");
        videoElement.src = uri; // INCOMPLETE // Marshal from Unity string
        videoElement.currentTime =  startTime / 1e+9;
    },

    NRGetFrameSize : function (reader, widthPtr, heightPtr) {

    },

    NRCopyNextFrame : function (reader, dstBufferPtr, byteSizePtr, timestampPtr) {
        // 
    },

    NRDispose : function (reader) {

    }
};

autoAddDeps(NatReaderWebGL, "$sharedInstance");

mergeInto(LibraryManager.library, NatReaderWebGL);