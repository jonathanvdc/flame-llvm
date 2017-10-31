files = \
	*.cs \
	IO/*.cs \
	Text/*.cs \
	Threading/*.cs \
	Runtime/InteropServices/*.cs

libs = ../runtime/bin/flo/runtime.flo

common-options = -asm-name=stdlib-primitives -asm-version=0.1.0.0

.PHONY: all flo clean
all: flo

flo: bin/flo/stdlib-primitives.flo

clean:
	rm -rf bin/

include ../flame-make-scripts/use-ecsc.mk

bin/flo/stdlib-primitives.flo: $(files) $(libs) | ecsc
	$(ECSC) $(files) -platform ir -environment llvm -Wno-build -rt-libs $(libs) -o $@ $(common-options)
