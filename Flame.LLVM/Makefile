files = \
	Codegen/*.cs \
	Passes/*.cs \
	Properties/*.cs \
	*.cs

libs = \
	../libs/*.dll \
	../packages/Flame.Compiler.0.9.15/lib/net45/*.dll \
	../packages/Flame.Optimization.0.9.15/lib/net45/*.dll

common-options = -asm-name=Flame.LLVM -asm-version=0.1.0.0

all: dll flo

dll: bin/clr/Flame.LLVM.dll
flo: bin/flo/Flame.LLVM.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/Flame.LLVM.dll: $(files) $(libs) $(native-libs)
	ecsc $(files) -platform clr -o $@ -O3 $(common-options) --libs $(libs)

bin/flo/Flame.LLVM.flo: $(files) $(libs) $(native-libs)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common-options) --libs $(libs)
