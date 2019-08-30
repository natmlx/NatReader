/* 
*   NatReader
*   Copyright (c) 2019 Yusuf Olokoba
*/

const NatReaderWebGL = {

    $sharedInstance : {

    },
};

autoAddDeps(NatReaderWebGL, "$sharedInstance");

mergeInto(LibraryManager.library, NatReaderWebGL);