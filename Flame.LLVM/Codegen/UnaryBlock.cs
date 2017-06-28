using System;
using Flame.Compiler;
using LLVMSharp;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that performs a unary operation.
    /// </summary>
    public sealed class UnaryBlock : CodeBlock
    {
        public UnaryBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Operand,
            IType Type,
            Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef> BuildUnary)
        {
            this.codeGen = CodeGenerator;
            this.operand = Operand;
            this.resultType = Type;
            this.build = BuildUnary;
        }

        private ICodeGenerator codeGen;
        private CodeBlock operand;
        private IType resultType;
        private Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef> build;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var operandCodegen = operand.Emit(BasicBlock);
            return new BlockCodegen(
                operandCodegen.BasicBlock,
                build(operandCodegen.BasicBlock.Builder, operandCodegen.Value, "tmp"));
        }
    }
}

