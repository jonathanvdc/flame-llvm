# Contributing to `flame-llvm`

There are lots of ways for you to help out with making `flame-llvm` successful:

  * Open an issue to report a bug.
  * Write a C# program that compiles to native code using `ecsc` and `flame-llvm`. Open an issue to tell me about it.
  * Fork `flame-llvm`, implement something and send a pull request. Some ideas to get you started:
    1. Implement a BCL API, for example, `Dictionary<TKey, TValue>`.
    2. Create a test case for either the compiler or the BCL.
    3. Optimize an already-implemented BCL API.
    4. Implement a compiler feature.

If you'd like to work with `flame-llvm` but end up getting stuck on something/getting confused/not knowing what to do, then just drop me an issue. I'd be happy to offer whatever help I can.

## Contributing standard library code

A BCL implementation is essential to making `flame-llvm` usable, so any and all contributions that help improve the standard library are most welcome. However, if you're considering contributing standard library code, then **please take a brief look at the [philosophy section](stdlib/README.md#philosophy) of the standard library's README.** It contains a few design guidelines which your contribution should (ideally) respect.
