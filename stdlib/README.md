# The `flame-llvm` standard library

This is an implementation of a small but growing subset of functionality from the .NET base class library (BCL). The goal is to offer source-level compatibility with C# code written for traditional CLR implementations without compromising on `flame-llvm`'s promise to produce a small, self-contained executable with quick start-up times.

## Philosophy

Traditional BCL implementations are expansive, highly fine-tuned beasts. The `flame-llvm` standard library aims to be a simple, concise, and maintainable BCL implementation that interfaces with the native toolchain's libraries through a consistent wrapper interface&mdash;the `primitives` library.

Here are some ground rules:

  * Code should be kept simple and straightforward.
  * Try to keep individual libraries lean and minimize their dependencies.
    * This reduces the coupling between standard library classes and, consequently, the number of classes required at run-time. The result is a smaller executable.
    * As a special instance of this rule: don't define non-essential classes such as `Console` and `HashSet<T>` in `corlib`. Put them in `system-console` and `system-collections`, respectively.
  * Only the `primitives` library is allowed to declare `extern` values.
  * Try to avoid overspecializing `primitives` to the libraries it interfaces with.

## Components

The `flame-llvm` standard library consists of the following components:

  * `primitives` is a platform-neutral wrapper around platform-specific libraries. It is also the only standard library component that is allowed to interface with native libraries. All other libraries must go through `primitives`.

  * `corlib` defines `Object`, `Environment`, and other basic functionality. It is not a full BCL implementation, but rather a collection of essentials which other libraries can extend.

  * `system-console` define the `Console` class, which handles standard input, error and output.

  * `system-io` defines IO-related classes, such as `Stream` and `TextWriter`.

  * `system-text` supplies text encoding and decoding functionality. The classes it defines include: `Encoding`, `UTF8Encoding`, etc.
