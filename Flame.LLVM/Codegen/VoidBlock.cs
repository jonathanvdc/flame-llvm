using System;
using Flame.Compiler;
using LLVMSharp;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that returns no value.
    /// </summary>
    public sealed class VoidBlock : CodeBlock
    {
        public VoidBlock(ICodeGenerator CodeGenerator)
        {
            this.codeGen = CodeGenerator;
        }

        private ICodeGenerator codeGen;

        public override ICodeGenerator CodeGenerator => codeGen;

        public override IType Type => PrimitiveTypes.Void;

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(BasicBlock, default(LLVMValueRef));
        }
    }
}