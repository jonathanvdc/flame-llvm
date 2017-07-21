using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that produces a pointer to an array dimension field.
    /// </summary>
    public sealed class GetDimensionPtrBlock : CodeBlock
    {
        /// <summary>
        /// Creates a block that produces a pointer to the nth dimension
        /// field of the array pointed to.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="ArrayPointer">
        /// A code block that creates a pointer to an array.
        /// </param>
        /// <param name="DimensionIndex">
        /// The index of the dimension to which a pointer should be created.
        /// </param>
        public GetDimensionPtrBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock ArrayPointer,
            int DimensionIndex)
        {
            this.codeGen = CodeGenerator;
            this.ArrayPointer = ArrayPointer;
            this.DimensionIndex = DimensionIndex;
        }

        /// <summary>
        /// Gets a code block that creates a pointer to an array.
        /// </summary>
        /// <returns>A code block that creates a pointer to an array.</returns>
        public CodeBlock ArrayPointer { get; private set; }

        /// <summary>
        /// Gets the index of the dimension to which a pointer should be created.
        /// </summary>
        /// <returns>The dimension index.</returns>
        public int DimensionIndex { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Int32;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var baseAddressResult = ArrayPointer.Emit(BasicBlock);
            BasicBlock = baseAddressResult.BasicBlock;
            return new BlockCodegen(
                BasicBlock,
                BuildStructGEP(
                    BasicBlock.Builder,
                    baseAddressResult.Value,
                    (uint)DimensionIndex,
                    "dimension_ptr_tmp"));
        }
    }
}