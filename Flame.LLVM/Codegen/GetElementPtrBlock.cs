using System;
using System.Collections.Generic;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that creates a GEP instruction.
    /// </summary>
    public sealed class GetElementPtrBlock : CodeBlock
    {
        /// <summary>
        /// Creates a GEP block from the given base address and indices.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="BaseAddress">The base address.</param>
        /// <param name="Indices">The GEP indices.</param>
        /// <param name="Type">The GEP's result type.</param>
        public GetElementPtrBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock BaseAddress,
            IReadOnlyList<CodeBlock> Indices,
            IType Type)
        {
            this.codeGen = CodeGenerator;
            this.BaseAddress = BaseAddress;
            this.Indices = Indices;
            this.resultType = Type;
        }

        /// <summary>
        /// Gets the base address for the GEP instruction.
        /// </summary>
        /// <returns>The base address.</returns>
        public CodeBlock BaseAddress { get; private set; }

        /// <summary>
        /// Gets the indices for the GEP instruction.
        /// </summary>
        /// <returns>The GEP indices.</returns>
        public IReadOnlyList<CodeBlock> Indices { get; private set; }

        private ICodeGenerator codeGen;
        private IType resultType;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var ptrResult = BaseAddress.Emit(BasicBlock);
            BasicBlock = ptrResult.BasicBlock;
            var indexVals = new LLVMValueRef[Indices.Count];
            for (int i = 0; i < indexVals.Length; i++)
            {
                var indexResult = Indices[i].Emit(BasicBlock);
                BasicBlock = indexResult.BasicBlock;
                indexVals[i] = indexResult.Value;
            }
            return new BlockCodegen(
                BasicBlock,
                BuildGEP(BasicBlock.Builder, ptrResult.Value, indexVals, "gep_tmp"));
        }
    }
}