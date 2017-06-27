files = \
	Properties/*.cs \
	*.cs

libs = \
	../libs/*.dll

native-libs = \
	../native-libs/*

common-options = -asm-name=flame-llvm -asm-version=0.1.0.0

all: exe flo

exe: bin/clr/flame-llvm.exe
flo: bin/flo/flame-llvm.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/flame-llvm.exe: $(files) $(libs) $(native-libs)
	ecsc $(files) -platform clr -o $@ -O3 $(common-options) --libs $(libs)
	cp $(native-libs) bin/clr/

bin/flo/flame-llvm.flo: $(files) $(libs) $(native-libs)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common-options) --libs $(libs)
