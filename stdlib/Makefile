corlib-files = \
	corlib/*.cs \
	../stdlib-primitives/bin/flo/stdlib-primitives.flo

runtime-lib = ../runtime/bin/flo/runtime.flo

common-options = -asm-name=stdlib -asm-version=0.1.0.0

all: flo

flo: bin/flo/corlib.flo

clean:
	rm -rf bin/

bin/flo/corlib.flo: $(corlib-files) $(runtime-lib)
	ecsc $(corlib-files) -platform ir -environment standalone -Wno-build -rt-libs $(runtime-lib) -o $@ $(common-options)
