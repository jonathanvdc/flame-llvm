using Flame.Compiler;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that marks a basic block with a label.
    /// </summary>
    public sealed class MarkLabelBlock : CodeBlock
    {
        /// <summary>
        /// Creates a code block that marks a basic block with a label.
        /// </summary>
        /// <param name="CodeGenerator">The code generator.</param>
        /// <param name="Label">The label of the basic block to mark.</param>
        public MarkLabelBlock(ICodeGenerator CodeGenerator, UniqueTag Label)
        {
            this.codeGen = CodeGenerator;
            this.Label = Label;
        }

        /// <summary>
        /// Gets the label to mark.
        /// </summary>
        /// <returns>The label to mark.</returns>
        public UniqueTag Label { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Void;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var markedBlock = BasicBlock.FunctionBody.GetOrCreateLabeledBlock(Label);
            BuildBr(BasicBlock.Builder, markedBlock.Block);
            return new BlockCodegen(markedBlock);
        }
    }
}