exe:
	make -C Flame.LLVM flo
	make -C flame-llvm exe
	make -C runtime native

all:
	make -C Flame.LLVM all
	make -C flame-llvm all
	make -C runtime all

dll:
	make -C Flame.LLVM all
	make -C flame-llvm exe

flo:
	make -C Flame.LLVM flo
	make -C flame-llvm flo
	make -C runtime flo

clean:
	make -C Flame.LLVM clean
	make -C flame-llvm clean
	make -C runtime clean

test: exe
	compare-test tests/all.test --flame-llvm=$(shell pwd)/flame-llvm/bin/clr/flame-llvm.exe --clang=clang-3.8 -j
