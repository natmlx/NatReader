/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

namespace NatReader {

    using System;

    public interface IMediaReader : IDisposable {
        void StartReading (string url);
    }
}