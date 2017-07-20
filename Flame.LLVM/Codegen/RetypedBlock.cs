using Flame.Compiler;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that defers to another block for codegen,
    /// but overrides said block's type with a type of its own.
    /// </summary>
    public sealed class RetypedBlock : CodeBlock
    {
        /// <summary>
        /// Creates a block that defers to the given value for codegen,
        /// but reports the specified type.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Value">The value block to which the retyped block defers for codegen.</param>
        /// <param name="Type">The type reported by the retyped block.</param>
        public RetypedBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Value,
            IType Type)
        {
            this.codeGen = CodeGenerator;
            this.Value = Value;
            this.resultType = Type;
        }

        /// <summary>
        /// Gets the block that produces this block's value.
        /// </summary>
        /// <returns>The value block.</returns>
        public CodeBlock Value { get; private set; }

        private IType resultType;

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return Value.Emit(BasicBlock);
        }
    }
}