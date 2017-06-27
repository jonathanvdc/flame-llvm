using System;
using System.Runtime.InteropServices;
using Flame.Front.Cli;
using Flame.Front.Target;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildTargetParsers.Parser.RegisterParser(new LLVMBuildTargetParser());
            var compiler = new ConsoleCompiler("flame-llvm", "the Flame IR -> LLVM compiler", "https://github.com/jonathanvdc/flame-llvm/releases");
            Environment.Exit(compiler.Compile(args));
        }
    }
}
