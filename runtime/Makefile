files = \
	*.cs

common-options = -asm-name=runtime -asm-version=0.1.0.0

all: flo native

flo: bin/flo/runtime.flo
native: bin/native/runtime.o

clean:
	rm -rf bin/

bin/flo/runtime.flo: $(files)
	ecsc $(files) -platform ir -Wno-build -o $@ $(common-options)

bin/native/runtime.ll: bin/flo/runtime.flo
	flame-llvm bin/flo/runtime.flo -platform llvm -Wno-build -o $@

bin/native/runtime-opt.ll: bin/native/runtime.ll
	opt-3.8 bin/native/runtime.ll -O3 -S -o $@

bin/native/runtime.o: bin/native/runtime-opt.ll
	llc-3.8 bin/native/runtime-opt.ll -filetype=obj -o bin/native/runtime.o
