using System;
using Flame.Compiler;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that retrieves a tagged value.
    /// </summary>
    public sealed class TaggedValueBlock : CodeBlock
    {
        public TaggedValueBlock(
            ICodeGenerator CodeGenerator,
            UniqueTag Tag,
            IType Type)
        {
            this.codeGen = CodeGenerator;
            this.Tag = Tag;
            this.valType = Type;
        }

        private ICodeGenerator codeGen;
        private IType valType;

        /// <summary>
        /// Gets the tag of the value to retrieve.
        /// </summary>
        /// <returns>The tag.</returns>
        public UniqueTag Tag { get; private set; }

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => valType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(BasicBlock, BasicBlock.FunctionBody.GetTaggedValue(Tag));
        }
    }
}