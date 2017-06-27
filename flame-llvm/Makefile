files = \
	Properties/*.cs \
	*.cs

libs = \
	../libs/*.dll

native-libs = \
	$(shell find ../native-libs/ -type f)

common-options = -asm-name=flame-llvm -asm-version=0.1.0.0

all: exe flo

exe: bin/clr/flame-llvm.exe
flo: bin/flo/flame-llvm.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/flame-llvm.exe: $(files) $(libs) $(native-libs)
	ecsc $(files) -platform clr -o $@ -O3 $(common-options) --libs $(libs)
	if [ ! -z $(native-libs) ]; then cp -t bin/clr/ $(native-libs); fi

bin/flo/flame-llvm.flo: $(files) $(libs) $(native-libs)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common-options) --libs $(libs)
