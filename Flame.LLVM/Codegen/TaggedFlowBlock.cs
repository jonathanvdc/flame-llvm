using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that creates a tagged flow block.
    /// </summary>
    public sealed class TaggedFlowBlock : CodeBlock
    {
        /// <summary>
        /// Creates a tagged flow block from the given tag and body.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates the block.</param>
        /// <param name="Tag">A tag to which 'break' and 'continue' blocks can refer.</param>
        /// <param name="Body">The contents of the block.</param>
        public TaggedFlowBlock(
            ICodeGenerator CodeGenerator,
            UniqueTag Tag,
            CodeBlock Body)
        {
            this.codeGen = CodeGenerator;
            this.Tag = Tag;
            this.Body = Body;
        }

        /// <summary>
        /// Gets the tag for this block, to which 'break' and 'continue' blocks
        /// can refer.
        /// </summary>
        /// <returns>The tag for this block.</returns>
        public UniqueTag Tag { get; private set; }

        /// <summary>
        /// Gets the contents of this block.
        /// </summary>
        /// <returns>The contents of this block.</returns>
        public CodeBlock Body { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => throw new NotImplementedException();

        /// <inheritdoc/>
        public override IType Type => Body.Type;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var continueBlock = BasicBlock.FunctionBody.AppendBasicBlock("continue_block");
            var breakBlock = BasicBlock.FunctionBody.AppendBasicBlock("break_block");
            BasicBlock.FunctionBody.TagFlowBlock(Tag, breakBlock.Block, continueBlock.Block);

            BuildBr(BasicBlock.Builder, continueBlock.Block);
            var bodyResult = Body.Emit(continueBlock);
            BuildBr(bodyResult.BasicBlock.Builder, breakBlock.Block);
            return new BlockCodegen(breakBlock, bodyResult.Value);
        }
    }
}