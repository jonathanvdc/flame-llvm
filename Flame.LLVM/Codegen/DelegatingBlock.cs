using System;
using Flame.Compiler;
using LLVMSharp;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that uses a delegate to implement
    /// code generation.
    /// </summary>
    public sealed class DelegatingBlock : CodeBlock
    {
        public DelegatingBlock(
            LLVMCodeGenerator CodeGenerator,
            IType Type,
            Func<LLVMValueRef, LLVMBuilderRef, BlockCodegen> Emit)
        {
            this.codeGen = CodeGenerator;
            this.instrType = Type;
            this.impl = Emit;
        }

        private LLVMCodeGenerator codeGen;
        private IType instrType;
        private Func<LLVMValueRef, LLVMBuilderRef, BlockCodegen> impl;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => instrType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(LLVMValueRef Function, LLVMBuilderRef BasicBlock)
        {
            return impl(Function, BasicBlock);
        }
    }
}

