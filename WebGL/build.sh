#!/bin/bash

readonly BUILD_PATH=Assets/NatReader/Plugins/WebGL

# Uglify
uglifyjs WebGL/NatReader.js --compress --mangle --keep-fnames -o $BUILD_PATH/NatReader.jslib
