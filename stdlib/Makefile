runtime-lib = ../runtime/bin/flo/runtime-csharp.flo

primitives-files = \
	primitives/*.cs \
	primitives/InteropServices/*.cs \
	primitives/IO/*.cs \
	primitives/Text/*.cs \
	primitives/Threading/*.cs

primitives-libs = $(runtime-lib)

corlib-files = \
	corlib/*.cs \
	corlib/Collections.Generic/*.cs \
	corlib/Runtime.InteropServices/*.cs \
	corlib/Threading/*.cs \
	bin/flo/primitives.flo

corlib-libs = $(runtime-lib)

system-text-files = \
	system-text/*.cs

system-text-libs = $(runtime-lib) bin/flo/corlib.flo

system-io-files = \
	system-io/*.cs

system-io-libs = $(runtime-lib) bin/flo/corlib.flo bin/flo/system-text.flo

system-console-files = \
	system-console/*.cs

system-console-libs = $(runtime-lib) bin/flo/corlib.flo bin/flo/system-text.flo bin/flo/system-io.flo

common-options = -asm-version=0.1.0.0

.PHONY: all flo clean
all: flo

flo: bin/flo/primitives.flo bin/flo/corlib.flo bin/flo/system-text.flo bin/flo/system-io.flo bin/flo/system-console.flo

clean:
	rm -rf bin/

include ../flame-make-scripts/use-ecsc.mk

bin/flo/primitives.flo: $(primitives-files) $(primitives-libs) | ecsc
	$(ECSC) $(primitives-files) \
		-platform ir -environment llvm -Wall -Wextra -Wno-build \
		-rt-libs $(primitives-libs) \
		-o $@ \
		$(common-options) -asm-name=primitives

bin/flo/corlib.flo: $(corlib-files) $(corlib-libs) | ecsc
	$(ECSC) $(corlib-files) \
		-platform ir -environment llvm -Wall -Wextra -Wno-build \
		-rt-libs $(corlib-libs) \
		-o $@ \
		$(common-options) -asm-name=stdlib

bin/flo/system-text.flo: $(system-text-files) $(system-text-libs) | ecsc
	$(ECSC) $(system-text-files) \
		-platform ir -environment llvm -Wall -Wextra -Wno-build \
		-rt-libs $(system-text-libs) \
		-o $@ \
		$(common-options) -asm-name=system-text

bin/flo/system-io.flo: $(system-io-files) $(system-io-libs) | ecsc
	$(ECSC) $(system-io-files) \
		-platform ir -environment llvm -Wall -Wextra -Wno-build \
		-rt-libs $(system-io-libs) \
		-o $@ \
		$(common-options) -asm-name=system-io

bin/flo/system-console.flo: $(system-console-files) $(system-console-libs) | ecsc
	$(ECSC) $(system-console-files) \
		-platform ir -environment llvm -Wall -Wextra -Wno-build \
		-rt-libs $(system-console-libs) \
		-o $@ \
		$(common-options) -asm-name=system-console
