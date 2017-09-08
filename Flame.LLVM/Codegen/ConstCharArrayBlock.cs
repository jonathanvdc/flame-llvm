using Flame.Compiler;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that produces a constant character array
    /// based on the contents of a string.
    /// </summary>
    public sealed class ConstCharArrayBlock : CodeBlock
    {
        public ConstCharArrayBlock(
            LLVMCodeGenerator CodeGenerator,
            string Data)
        {
            this.codeGen = CodeGenerator;
            this.Data = Data;
        }

        private LLVMCodeGenerator codeGen;

        /// <summary>
        /// Gets the data to fill this constant character array with.
        /// </summary>
        /// <returns>The data.</returns>
        public string Data { get; private set; }

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Char.MakeArrayType(1);

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(BasicBlock, BasicBlock.FunctionBody.Module.GetConstCharArray(Data));
        }
    }
}