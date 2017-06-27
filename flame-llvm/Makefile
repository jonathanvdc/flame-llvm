files = \
	Program.cs

libs = \
	../libs/*.dll

common_options = -asm-name=flame-llvm -asm-version=0.1.0.0

all: exe flo

exe: bin/clr/flame-llvm.exe
flo: bin/flo/flame-llvm.flo

clean:
	rm -rf bin/
	rm -rf obj/

bin/clr/flame-llvm.exe: $(files)
	ecsc $(files) -platform clr -o $@ -O3 $(common_options) --libs $(libs)

bin/flo/flame-llvm.flo: $(files)
	ecsc $(files) -platform ir -runtime clr -o $@ $(common_options) --libs $(libs)
