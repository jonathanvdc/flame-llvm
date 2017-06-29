using System;
using Flame.Compiler;
using LLVMSharp;

namespace Flame.LLVM.Codegen
{
    public sealed class ComparisonBlock : CodeBlock
    {
        public ComparisonBlock(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock Left,
            CodeBlock Right,
            LLVMIntPredicate Predicate)
        {
            this.codeGen = CodeGenerator;
            this.lhs = Left;
            this.rhs = Right;
            this.predicate = Predicate;
        }

        private LLVMCodeGenerator codeGen;
        private LLVMIntPredicate predicate;
        private CodeBlock lhs;
        private CodeBlock rhs;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Boolean;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var lhsCodegen = lhs.Emit(BasicBlock);
            var rhsCodegen = rhs.Emit(lhsCodegen.BasicBlock);
            return new BlockCodegen(
                rhsCodegen.BasicBlock,
                LLVMSharp.LLVM.BuildICmp(
                    rhsCodegen.BasicBlock.Builder,
                    predicate,
                    lhsCodegen.Value,
                    rhsCodegen.Value,
                    "tmp"));
        }
    }
}

