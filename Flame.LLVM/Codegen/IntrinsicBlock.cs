using Flame.Compiler;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that wraps an intrinsic value.
    /// </summary>
    public sealed class IntrinsicBlock : CodeBlock
    {
        /// <summary>
        /// Creates an intrinsic block that wraps the given intrinsic.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates the block.</param>
        /// <param name="Intrinsic">The intrinsic to wrap.</param>
        public IntrinsicBlock(ICodeGenerator CodeGenerator, IntrinsicValue Intrinsic)
        {
            this.codeGenerator = CodeGenerator;
            this.Intrinsic = Intrinsic;
        }

        /// <summary>
        /// Gets the intrinsic value wrapped by this code block.
        /// </summary>
        /// <returns>The intrinsic value.</returns>
        public IntrinsicValue Intrinsic { get; private set; }

        private ICodeGenerator codeGenerator;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGenerator;

        /// <inheritdoc/>
        public override IType Type => Intrinsic.Type;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(
                BasicBlock,
                BasicBlock.FunctionBody.Module.Declare(Intrinsic));
        }
    }
}