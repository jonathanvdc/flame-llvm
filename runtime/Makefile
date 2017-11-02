csharp-files = \
	csharp/*.cs

c-files = \
	c/*.c

common-options = -asm-name=runtime -asm-version=0.1.0.0

.PHONY: all flo native clean
all: flo native

flo: bin/flo/runtime-csharp.flo
native: bin/native/runtime.o

clean:
	rm -rf bin/

include ../flame-make-scripts/use-ecsc.mk

bin/native/runtime-c.ll: $(c-files)
	clang-4.0 $(c-files) -emit-llvm -S -o $@

bin/flo/runtime-csharp.flo: $(csharp-files) | ecsc
	$(ECSC) $(csharp-files) -platform ir -environment llvm -Wno-build -o $@ $(common-options)

bin/native/runtime-csharp.ll: bin/flo/runtime-csharp.flo
	mono ../flame-llvm/bin/clr/flame-llvm.exe bin/flo/runtime-csharp.flo -platform llvm -Wno-build -fintegrated-runtime -o $@

bin/native/runtime.ll: bin/native/runtime-c.ll bin/native/runtime-csharp.ll
	llvm-link-4.0 bin/native/runtime-c.ll bin/native/runtime-csharp.ll -S -o $@ 2> /dev/null

bin/native/runtime-opt.ll: bin/native/runtime.ll
	opt-4.0 bin/native/runtime.ll -O3 -S -o $@

bin/native/runtime.o: bin/native/runtime-opt.ll
	llc-4.0 bin/native/runtime-opt.ll -filetype=obj -o bin/native/runtime.o
