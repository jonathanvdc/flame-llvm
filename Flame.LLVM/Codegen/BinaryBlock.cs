using System;
using Flame.Compiler;
using LLVMSharp;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that performs a binary operation.
    /// </summary>
    public sealed class BinaryBlock : CodeBlock
    {
        public BinaryBlock(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock Left,
            CodeBlock Right,
            IType Type,
            Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef> BuildBinary)
        {
            this.codeGen = CodeGenerator;
            this.lhs = Left;
            this.rhs = Right;
            this.resultType = Type;
            this.build = BuildBinary;
        }

        private LLVMCodeGenerator codeGen;
        private CodeBlock lhs;
        private CodeBlock rhs;
        private IType resultType;
        private Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef> build;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var lhsCodegen = lhs.Emit(BasicBlock);
            var rhsCodegen = rhs.Emit(lhsCodegen.BasicBlock);
            return new BlockCodegen(
                rhsCodegen.BasicBlock,
                build(rhsCodegen.BasicBlock.Builder, lhsCodegen.Value, rhsCodegen.Value, "tmp"));
        }
    }
}

