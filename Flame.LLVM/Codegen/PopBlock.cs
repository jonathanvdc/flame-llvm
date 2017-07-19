using System;
using Flame.Compiler;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that ignores a value.
    /// </summary>
    public sealed class PopBlock : CodeBlock
    {
        /// <summary>
        /// Creates a block that ignores the given value.
        /// </summary>
        /// <param name="CodeGenerator">The code generator.</param>
        /// <param name="Value">The value to ignore.</param>
        public PopBlock(ICodeGenerator CodeGenerator, CodeBlock Value)
        {
            this.codeGen = CodeGenerator;
            this.Value = Value;
        }

        /// <summary>
        /// Gets the value to ignore.
        /// </summary>
        /// <returns>The value to ignore.</returns>
        public CodeBlock Value { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Void;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(Value.Emit(BasicBlock).BasicBlock);
        }
    }
}