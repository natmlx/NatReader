/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba.
*/

namespace NatReader.Internal {

    using System;

    public interface IMediaEnumerator : IDisposable {
        bool CopyNextFrame (IntPtr dstBuffer, out int byteSize, out long timestamp);
    }
}