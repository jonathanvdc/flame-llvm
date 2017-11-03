runtime-lib = ../runtime/bin/flo/runtime.flo

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
	bin/flo/primitives.flo

corlib-libs = $(runtime-lib)

system-console-files = \
	system-console/*.cs

system-console-libs = $(runtime-lib) bin/flo/corlib.flo bin/flo/system-text.flo bin/flo/system-io.flo

system-io-files = \
	system-io/*.cs

system-io-libs = $(runtime-lib) bin/flo/corlib.flo bin/flo/system-text.flo

system-text-files = \
	system-text/*.cs

system-text-libs = $(runtime-lib) bin/flo/corlib.flo

system-threading-files = \
	system-threading/*.cs

system-threading-libs = $(runtime-lib) bin/flo/corlib.flo

common-options = \
	-Wall -Wextra -pedantic -Wno-build -Wno-unused-method \
	-asm-version=0.1.0.0

.PHONY: all flo clean
all: flo

flo: \
	bin/flo/primitives.flo \
	bin/flo/corlib.flo \
	bin/flo/system-console.flo \
	bin/flo/system-io.flo \
	bin/flo/system-text.flo \
	bin/flo/system-threading.flo

clean:
	rm -rf bin/

include ../flame-make-scripts/use-ecsc.mk

bin/flo/primitives.flo: $(primitives-files) $(primitives-libs) | ecsc
	$(ECSC) $(primitives-files) \
		-platform ir -environment llvm \
		-rt-libs $(primitives-libs) \
		-o $@ \
		$(common-options) -asm-name=primitives

bin/flo/corlib.flo: $(corlib-files) $(corlib-libs) | ecsc
	$(ECSC) $(corlib-files) \
		-platform ir -environment llvm \
		-rt-libs $(corlib-libs) \
		-o $@ \
		$(common-options) -asm-name=stdlib

bin/flo/system-console.flo: $(system-console-files) $(system-console-libs) | ecsc
	$(ECSC) $(system-console-files) \
		-platform ir -environment llvm \
		-rt-libs $(system-console-libs) \
		-o $@ \
		$(common-options) -asm-name=system-console

bin/flo/system-io.flo: $(system-io-files) $(system-io-libs) | ecsc
	$(ECSC) $(system-io-files) \
		-platform ir -environment llvm \
		-rt-libs $(system-io-libs) \
		-o $@ \
		$(common-options) -asm-name=system-io

bin/flo/system-text.flo: $(system-text-files) $(system-text-libs) | ecsc
	$(ECSC) $(system-text-files) \
		-platform ir -environment llvm \
		-rt-libs $(system-text-libs) \
		-o $@ \
		$(common-options) -asm-name=system-text

bin/flo/system-threading.flo: $(system-threading-files) $(system-threading-libs) | ecsc
	$(ECSC) $(system-threading-files) \
		-platform ir -environment llvm \
		-rt-libs $(system-threading-libs) \
		-o $@ \
		$(common-options) -asm-name=system-threading
