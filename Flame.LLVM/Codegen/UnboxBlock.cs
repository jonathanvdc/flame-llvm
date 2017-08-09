using System;
using Flame.Compiler;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that creates a pointer to the contents of
    /// a box.
    /// </summary>
    public sealed class UnboxBlock : CodeBlock
    {
        /// <summary>
        /// Creates a code block that produces a pointer to the contents of
        /// a box
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates the block.</param>
        /// <param name="Box">A pointer to the box to open.</param>
        /// <param name="ElementType">The type of value in the box.</param>
        public UnboxBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Box,
            IType ElementType)
        {
            this.codeGen = CodeGenerator;
            this.Box = Box;
            this.elemType = ElementType;
            this.resultType = ElementType.MakePointerType(PointerKind.TransientPointer);
        }

        /// <summary>
        /// Gets a pointer to the box to unbox.
        /// </summary>
        /// <returns>The box.</returns>
        public CodeBlock Box { get; private set; }

        private IType resultType;
        private IType elemType;

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var boxCodegen = Box.Emit(BasicBlock);
            BasicBlock = boxCodegen.BasicBlock;
            return new BlockCodegen(
                BasicBlock,
                BuildStructGEP(
                    BasicBlock.Builder,
                    BuildBitCast(
                        BasicBlock.Builder,
                        boxCodegen.Value,
                        BasicBlock.FunctionBody.Module.Declare(
                            elemType.MakePointerType(PointerKind.BoxPointer)),
                        "box_ptr"),
                        1,
                        "unboxed_ptr"));
        }
    }
}