# `flame-llvm`: an LLVM back-end for Flame

[![Build Status](https://travis-ci.org/jonathanvdc/flame-llvm.svg?branch=master)](https://travis-ci.org/jonathanvdc/flame-llvm)

`flame-llvm` is a tool that compiles [Flame](https://github.com/jonathanvdc/Flame) IR to [LLVM](http://llvm.org) IR using [LLVMSharp](https://github.com/Microsoft/LLVMSharp). Since [`ecsc`](https://github.com/jonathanvdc/ecsc) can compile C# source code to Flame IR, `flame-llvm` can (indirectly) turn C# programs into fast, native binaries.

## Compiling `flame-llvm`

To use `flame-llvm`, you'll need to have the following installed:

  * basic command-line tools like `bash`, `make`, `cp`, etc (in your `$PATH`),
  * a .NET framework implementation (for example, [Mono](http://www.mono-project.com/)),
  * `clang`,
  * the Boehm-Demers-Weiser GC (Debian package: `libgc-dev`),
  * the `libc++abi` library (or equivalent) and
  * **LLVM 4.0**'s `libLLVM.(so|dll|dylib)`, whichever is appropriate for your platform. It doesn't really matter where you/an installer/your package manager put it; we'll copy it to the output directory.

For Debian-based distributions, the following command should install all dependencies:

```bash
sudo apt install mono-devel llvm-4.0-dev clang-4.0 libc++abi-dev libgc-dev
```

Clone `flame-llvm`, including its submodules, like so

```bash
git clone --recursive https://github.com/jonathanvdc/flame-llvm
cd flame-llvm
```

Now configure the LLVM dependency and build `flame-llvm`.

```bash
./build-libs.sh /path/to/libLLVM.so
make
```

You probably also want to compile the standard library.

```bash
make stdlib
```

Alternatively, you can use the MSBuild solution file (`flame-llvm.sln`) to build `flame-llvm` and then manually copy `libLLVM.(so|dll|dylib)` to the output directory, but I recommend using the Makefile.

## Compiling a C# program

Compiling a program with `flame-llvm` consists of multiple steps so it's kind of tricky to get right. Assuming you've built the standard library, the following should work. You probably want to wrap all of this into a neat little makefile.

```bash
# First, compile the source code to Flame IR.
ecsc program.cs --platform ir -Wno-build --rt-libs /path/to/flame-llvm-repo/stdlib/bin/flo/corlib.flo -o program.flo

# Compile the Flame IR to LLVM IR, linking in the core library.
flame-llvm program.flo /path/to/flame-llvm-repo/stdlib/flo/corlib.flo --platform llvm -Wno-build --rt-libs /path/to/flame-llvm-repo/runtime/bin/flo/runtime.flo
-o program.ll

# Compile the LLVM IR to an executable. Link in the runtime and libgc.
clang program.ll /path/to/flame-llvm-repo/runtime/bin/native/runtime.o -lgc -lc++abi -Wno-override-module -o a.out
```

## Feature status

`flame-llvm` is not feature-complete yet, but it is mature enough to compile basic programs. Here's a feature list.

  - [x] integer arithmetic
  - [x] floating-point arithmetic
  - [x] control flow (`while`, `for`, `if`, `break`, `continue`, etc)
  - [x] direct function calls
  - [x] `extern` functions
  - [x] `struct` values
  - [x] `class` values
  - [x] `enum` values
  - [x] arrays
  - [x] support for strings
  - [x] conservative garbage collection (Boehm-Demers-Weiser GC)
  - [x] inheritance
  - [x] dynamic casts, `is`, `as`
  - [x] `virtual` calls
  - [x] `interface` calls
  - [x] generics
  - [x] boxing/unboxing
  - [x] `static` constructors
  - [x] exception handling (`throw`, `try`, `catch`, `finally`)&mdash;Itanium ABI (Linux, Mac OS X) only for now
  - [ ] finalizers
  - [ ] indirect function calls (delegates)
  - [ ] reflection
  - [ ] precise garbage collection
