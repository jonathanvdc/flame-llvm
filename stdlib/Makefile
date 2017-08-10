runtime-lib = ../runtime/bin/flo/runtime.flo

corlib-files = \
	corlib/*.cs \
	../stdlib-primitives/bin/flo/stdlib-primitives.flo

corlib-libs = $(runtime-lib)

system-console-files = \
	system-console/*.cs \
	../stdlib-primitives/bin/flo/stdlib-primitives.flo

system-console-libs = $(runtime-lib) bin/flo/corlib.flo

common-options = -asm-name=stdlib -asm-version=0.1.0.0

all: flo

flo: bin/flo/corlib.flo bin/flo/system-console.flo

clean:
	rm -rf bin/

bin/flo/corlib.flo: $(corlib-files) $(corlib-libs)
	ecsc $(corlib-files) -platform ir -environment standalone -Wno-build -rt-libs $(runtime-lib) -o $@ $(common-options)

bin/flo/system-console.flo: $(system-console-files) $(system-console-libs)
	ecsc $(system-console-files) -platform ir -environment standalone -Wno-build -rt-libs $(system-console-libs) -o $@ $(common-options)