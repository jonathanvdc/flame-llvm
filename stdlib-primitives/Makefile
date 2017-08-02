files = \
	IO/*.cs \
	Runtime/InteropServices/*.cs

libs = ../runtime/bin/flo/runtime.flo

common-options = -asm-name=stdlib-primitives -asm-version=0.1.0.0

all: flo

flo: bin/flo/stdlib-primitives.flo

clean:
	rm -rf bin/

bin/flo/stdlib-primitives.flo: $(files) $(libs)
	ecsc $(files) -platform ir -Wno-build -rt-libs $(libs) -o $@ $(common-options)
