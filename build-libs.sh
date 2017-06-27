#!/usr/bin/env bash
mkdir -p libs
mcs /target:library /out:libs/LLVMSharp.dll LLVMSharp/*.cs
