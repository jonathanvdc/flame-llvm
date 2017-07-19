exe:
	make -C Flame.LLVM flo
	make -C flame-llvm exe

all:
	make -C Flame.LLVM all
	make -C flame-llvm all

dll:
	make -C Flame.LLVM all
	make -C flame-llvm exe

flo:
	make -C Flame.LLVM flo
	make -C flame-llvm flo

clean:
	make -C Flame.LLVM clean
	make -C flame-llvm clean

test: exe
	compare-test tests/all.test --flame-llvm=$(shell pwd)/flame-llvm/bin/clr/flame-llvm.exe --clang=clang-3.8;
