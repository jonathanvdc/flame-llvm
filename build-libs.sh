#!/usr/bin/env bash
rm -rf native-libs
mkdir -p native-libs
if [ -z "$1" ]; then
  echo warning: libLLVM.so will not be copied to the output directory
  echo usage: build-libs.sh [/path/to/libLLVM.so]
else
  cp $1 native-libs/$(basename $1)
fi

mkdir -p libs
mcs /target:library /out:libs/LLVMSharp.dll LLVMSharp/*.cs
