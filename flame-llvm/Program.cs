using System;
using LLVMSharp;
using System.Runtime.InteropServices;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // Based on the SharpLLVM example program.

            LLVMBool False = new LLVMBool(0);
            LLVMModuleRef mod = ModuleCreateWithName("LLVMSharpIntro");

            LLVMTypeRef[] param_types = { Int32Type(), Int32Type() };
            LLVMTypeRef ret_type = FunctionType(Int32Type(), out param_types[0], 2, False);
            LLVMValueRef sum = AddFunction(mod, "sum", ret_type);

            LLVMBasicBlockRef entry = AppendBasicBlock(sum, "entry");

            LLVMBuilderRef builder = CreateBuilder();
            PositionBuilderAtEnd(builder, entry);
            LLVMValueRef tmp = BuildAdd(builder, GetParam(sum, 0), GetParam(sum, 1), "tmp");
            BuildRet(builder, tmp);

            IntPtr error;
            VerifyModule(mod, LLVMVerifierFailureAction.LLVMAbortProcessAction, out error);
            DisposeMessage(error);

            if (WriteBitcodeToFile(mod, "sum.bc") != 0)
            {
                Console.WriteLine("error writing bitcode to file, skipping");
            }

            DumpModule(mod);

            DisposeBuilder(builder);
        }
    }
}
