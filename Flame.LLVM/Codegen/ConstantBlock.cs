using System;
using Flame.Compiler;
using LLVMSharp;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that always creates the same value.
    /// </summary>
    public sealed class ConstantBlock : CodeBlock
    {
        public ConstantBlock(
            LLVMCodeGenerator CodeGenerator,
            IType Type,
            LLVMValueRef Value)
        {
            this.codeGen = CodeGenerator;
            this.instrType = Type;
            this.val = Value;
        }

        private LLVMCodeGenerator codeGen;
        private IType instrType;
        private LLVMValueRef val;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => instrType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(BasicBlock, val);
        }
    }
}

