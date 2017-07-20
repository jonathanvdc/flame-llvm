using System;
using Flame.Compiler;
using LLVMSharp;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that runs two blocks in sequence.
    /// </summary>
    public sealed class SequenceBlock : CodeBlock
    {
        public SequenceBlock(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock FirstBlock,
            CodeBlock SecondBlock)
        {
            this.codeGen = CodeGenerator;
            this.firstBlock = FirstBlock;
            this.secondBlock = SecondBlock;
            var firstType = FirstBlock.Type;
            var secondType = SecondBlock.Type;
            if (secondType == PrimitiveTypes.Void
                && firstType != PrimitiveTypes.Void)
            {
                this.secondIsResult = false;
                this.resultType = firstType;
            }
            else
            {
                this.secondIsResult = true;
                this.resultType = secondType;
            }
        }

        private LLVMCodeGenerator codeGen;
        private CodeBlock firstBlock;
        private CodeBlock secondBlock;
        private bool secondIsResult;
        private IType resultType;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var firstCodegen = firstBlock.Emit(BasicBlock);
            var secondCodegen = secondBlock.Emit(firstCodegen.BasicBlock);
            return new BlockCodegen(
                secondCodegen.BasicBlock,
                secondIsResult ? secondCodegen.Value : firstCodegen.Value);
        }
    }
}

