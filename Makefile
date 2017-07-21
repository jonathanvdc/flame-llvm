.PHONY: exe
exe:
	make -C Flame.LLVM flo
	make -C flame-llvm exe

.PHONY: all
all:
	make -C Flame.LLVM all
	make -C flame-llvm all

.PHONY: dll
dll:
	make -C Flame.LLVM all
	make -C flame-llvm exe

.PHONY: flo
flo:
	make -C Flame.LLVM flo
	make -C flame-llvm flo

.PHONY: runtime
runtime:
	make -C runtime all

.PHONY: clean
clean:
	make -C Flame.LLVM clean
	make -C flame-llvm clean
	make -C runtime clean

.PHONY: test
test: exe runtime
	compare-test tests/all.test --clang=clang-3.8 -j
