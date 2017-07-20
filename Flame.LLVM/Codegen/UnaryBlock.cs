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
            Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef> BuildUnary,
            Func<LLVMValueRef, LLVMValueRef> BuildConstUnary)
        {
            this.codeGen = CodeGenerator;
            this.operand = Operand;
            this.resultType = Type;
            this.build = BuildUnary;
            this.buildConst = BuildConstUnary;
        }

        public UnaryBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Operand,
            IType Type,
            Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef> BuildUnary)
            : this(CodeGenerator, Operand, Type, BuildUnary, null)
        { }

        private ICodeGenerator codeGen;
        private CodeBlock operand;
        private IType resultType;
        private Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef> build;
        private Func<LLVMValueRef, LLVMValueRef> buildConst;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var operandCodegen = operand.Emit(BasicBlock);
            if (buildConst != null && operandCodegen.Value.IsConstant())
            {
                return new BlockCodegen(
                    operandCodegen.BasicBlock,
                    buildConst(operandCodegen.Value));
            }
            else
            {
                return new BlockCodegen(
                    operandCodegen.BasicBlock,
                    build(operandCodegen.BasicBlock.Builder, operandCodegen.Value, "tmp"));
            }
        }
    }
}

