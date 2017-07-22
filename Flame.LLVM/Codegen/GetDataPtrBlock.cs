using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that produces a pointer to an array's data buffer.
    /// </summary>
    public sealed class GetDataPtrBlock : CodeBlock
    {
        /// <summary>
        /// Creates a block that produces a pointer to the data section
        /// of an array.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="ArrayPointer">
        /// A code block that creates a pointer to an array.
        /// </param>
        public GetDataPtrBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock ArrayPointer)
        {
            this.codeGen = CodeGenerator;
            this.ArrayPointer = ArrayPointer;
        }

        /// <summary>
        /// Gets a code block that creates a pointer to an array.
        /// </summary>
        /// <returns>A code block that creates a pointer to an array.</returns>
        public CodeBlock ArrayPointer { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type =>
            ArrayPointer.Type
            .AsArrayType().ElementType
            .MakePointerType(PointerKind.ReferencePointer);

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            // The data layout of an array is `{ i32, ..., [0 x <element type>] }`,
            // so the last field has the same index as the array's rank.
            // To compute our result, we'll create a pointer to that field.
            int rank = ArrayPointer.Type.AsArrayType().ArrayRank;
            var baseAddressResult = ArrayPointer.Emit(BasicBlock);
            BasicBlock = baseAddressResult.BasicBlock;
            return new BlockCodegen(
                BasicBlock,
                BuildBitCast(
                    BasicBlock.Builder,
                    BuildStructGEP(
                        BasicBlock.Builder,
                        baseAddressResult.Value,
                        (uint)rank,
                        "data_array_tmp"),
                    BasicBlock.FunctionBody.Module.Declare(Type),
                    "data_ptr_tmp"));
        }
    }
}