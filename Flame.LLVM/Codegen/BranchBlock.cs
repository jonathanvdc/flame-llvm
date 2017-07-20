using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that performs a 'break' or 'continue' operation.
    /// </summary>
    public sealed class BranchBlock : CodeBlock
    {
        /// <summary>
        /// Creates a branching block from a tag and a Boolean
        /// that distinguishes between 'break' and 'continue' blocks.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates the block.</param>
        /// <param name="Tag">The tag of the target block.</param>
        /// <param name="IsBreak"><c>true</c> creates a 'break' block, <c>false</c> creates a 'continue' block.</param>
        public BranchBlock(
            ICodeGenerator CodeGenerator,
            UniqueTag Tag,
            bool IsBreak)
        {
            this.codeGen = CodeGenerator;
            this.Tag = Tag;
            this.IsBreak = IsBreak;
        }

        /// <summary>
        /// Gets the tag of the block to which a branching
        /// operation is to be applied.
        /// </summary>
        /// <returns>The tag of a flow block.</returns>
        public UniqueTag Tag { get; private set; }

        /// <summary>
        /// Tells if this block performs a 'break' operation.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this block represents a 'break' operation;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool IsBreak { get; private set; }

        /// <summary>
        /// Tells if this block performs a 'continue' operation.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this block represents a 'continue' operation;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool IsContinue => !IsBreak;

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Void;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var targetBlock = IsBreak
                ? BasicBlock.FunctionBody.GetBreakBlock(Tag)
                : BasicBlock.FunctionBody.GetContinueBlock(Tag);

            BuildBr(BasicBlock.Builder, targetBlock);
            return new BlockCodegen(BasicBlock.FunctionBody.AppendBasicBlock());
        }
    }
}