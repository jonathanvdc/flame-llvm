# `flame-llvm`: an LLVM back-end for Flame

`flame-llvm` is a tool that compiles [Flame](https://github.com/jonathanvdc/Flame) IR to [LLVM](http://llvm.org) IR. Since [`ecsc`](https://github.com/jonathanvdc/ecsc) can compile C# source code to Flame IR, `flame-llvm` can (indirectly) be used to turn C# programs into fast, native binaries.

## Feature status

`flame-llvm` is very much a work in progress. Here's a list of features and their status.

  - [x] integer arithmetic
  - [x] control flow (`while`, `for`, `if`, `break`, `continue`, etc)
  - [x] direct function calls
  - [x] `extern` functions
  - [x] `struct` values
  - [x] `class` values
  - [x] arrays
  - [ ] inheritance
  - [ ] indirect function calls
  - [ ] `virtual` calls
  - [ ] floating-point arithmetic
  - [ ] garbage collection