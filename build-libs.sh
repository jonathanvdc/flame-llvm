#!/usr/bin/env bash
if [ -z "$1" ]; then
  echo error: too few arguments
  echo usage: build-libs.sh /path/to/libLLVM.so
  exit 1
fi

mkdir -p native-libs
cp $1 native-libs/libLLVM.so

mkdir -p libs
mcs /target:library /out:libs/LLVMSharp.dll LLVMSharp/*.cs
