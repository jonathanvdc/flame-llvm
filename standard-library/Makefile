files = \
	*.cs

common-options = -asm-name=stdlib -asm-version=0.1.0.0

all: flo native

flo: bin/flo/stdlib.flo
native: bin/native/stdlib.o

clean:
	rm -rf bin/

bin/flo/stdlib.flo: $(files)
	ecsc $(files) -platform ir -Wno-build -rt-libs ../runtime/bin/flo/runtime.flo -o $@ $(common-options)

bin/native/stdlib.ll: bin/flo/stdlib.flo
	flame-llvm bin/flo/stdlib.flo -platform llvm -Wno-build -rt-libs ../runtime/bin/flo/runtime.flo -o $@

bin/native/stdlib-opt.ll: bin/native/stdlib.ll
	opt-3.8 bin/native/stdlib.ll -O3 -S -o $@

bin/native/stdlib.o: bin/native/stdlib-opt.ll
	llc-3.8 bin/native/stdlib-opt.ll -filetype=obj -o bin/native/stdlib.o
