files = \
	Properties/*.cs \
	*.cs \
	../Flame.LLVM/bin/flo/Flame.LLVM.flo

libs = \
	../libs/*.dll \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.dll \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.Mdb.dll \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.Pdb.dll \
	../packages/Mono.Cecil.0.9.6.4/lib/net45/Mono.Cecil.Rocks.dll \
	../packages/Loyc.Essentials.24.3.0/lib/net45/Loyc.Essentials.dll \
	../packages/Loyc.Collections.24.3.0/lib/net45/Loyc.Collections.dll \
	../packages/Loyc.Syntax.24.3.0/lib/net45/Loyc.Syntax.dll \
	../packages/Loyc.Binary.0.1.8/lib/net45/Loyc.Binary.dll \
	../packages/Flame.Compiler.0.9.18/lib/net45/*.dll \
	../packages/Flame.Optimization.0.9.18/lib/net45/*.dll \
	../packages/Flame.Front.0.9.18/lib/net45/*.dll

native-libs = \
	$(shell find ../native-libs -type f)

common-options = -asm-name=flame-llvm -asm-version=0.1.0.0

.PHONY: all exe flo clean
all: exe flo

exe: bin/clr/flame-llvm.exe
flo: bin/flo/flame-llvm.flo

clean:
	rm -rf bin/
	rm -rf obj/

include ../flame-make-scripts/use-ecsc.mk

bin/clr/flame-llvm.exe: $(files) $(libs) $(native-libs) | ecsc
	$(ECSC) $(files) -platform clr -o $@ -O3 $(common-options) --libs $(libs)
	if [ ! -z $(native-libs) ]; then cp $(native-libs) bin/clr/; fi

bin/flo/flame-llvm.flo: $(files) $(libs) $(native-libs) | ecsc
	$(ECSC) $(files) -platform ir -runtime clr -o $@ $(common-options) --libs $(libs)
