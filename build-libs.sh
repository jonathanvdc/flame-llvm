#!/usr/bin/env bash
rm -rf native-libs
mkdir -p native-libs
if [ -z "$1" ]; then
  echo warning: no native library will be copied to the output directory
  echo usage: build-libs.sh [/path/to/libLLVM.so]
else
  cp $1 native-libs/libLLVM.so
fi

mkdir -p libs
mcs /target:library /out:libs/LLVMSharp.dll LLVMSharp/*.cs
