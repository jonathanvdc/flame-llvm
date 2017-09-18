files = \
	Codegen/*.cs \
	Passes/*.cs \
	Properties/*.cs \
	*.cs

libs = \
	../libs/*.dll \
	../packages/Flame.Compiler.0.10.2/lib/net45/*.dll \
	../packages/Flame.Optimization.0.10.2/lib/net45/*.dll

common-options = -asm-name=Flame.LLVM -asm-version=0.1.0.0

.PHONY: all dll flo clean
all: dll flo

dll: bin/clr/Flame.LLVM.dll
flo: bin/flo/Flame.LLVM.flo

clean:
	rm -rf bin/
	rm -rf obj/

include ../flame-make-scripts/use-ecsc.mk

bin/clr/Flame.LLVM.dll: $(files) $(libs) $(native-libs) | ecsc
	$(ECSC) $(files) -platform clr -o $@ -O3 $(common-options) --libs $(libs)

bin/flo/Flame.LLVM.flo: $(files) $(libs) $(native-libs) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr -o $@ $(common-options) --libs $(libs)
