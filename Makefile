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

runtime:
	make -C runtime all

clean:
	make -C Flame.LLVM clean
	make -C flame-llvm clean
	make -C runtime clean

test: exe runtime
	compare-test tests/all.test --clang=clang-3.8 -j
