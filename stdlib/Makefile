files = \
	*.cs \
	../stdlib-primitives/bin/flo/stdlib-primitives.flo

libs = ../runtime/bin/flo/runtime.flo

common-options = -asm-name=stdlib -asm-version=0.1.0.0

all: flo

flo: bin/flo/stdlib.flo

clean:
	rm -rf bin/

bin/flo/stdlib.flo: $(files) $(libs)
	ecsc $(files) -platform ir -environment standalone -Wno-build -rt-libs $(libs) -o $@ $(common-options)
