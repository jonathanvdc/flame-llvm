using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that gets the size of a type.
    /// </summary>
    public sealed class SizeOfBlock : CodeBlock
    {
        /// <summary>
        /// Creates a block that measures the size of a type.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="TypeToMeasure">Gets the type to measure.</param>
        public SizeOfBlock(
            ICodeGenerator CodeGenerator,
            IType TypeToMeasure)
            : this(CodeGenerator, TypeToMeasure, true)
        { }

        /// <summary>
        /// Creates a block that measures the size of a type.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="TypeToMeasure">Gets the type to measure.</param>
        /// <param name="IsReferenceSize">
        /// Tells if the size of a reference to a reference type is measured,
        /// rather than its underlying size.
        /// </param>
        public SizeOfBlock(
            ICodeGenerator CodeGenerator,
            IType TypeToMeasure,
            bool IsReferenceSize)
        {
            this.codeGen = CodeGenerator;
            this.TypeToMeasure = TypeToMeasure;
            this.IsReferenceSize = IsReferenceSize;
        }

        /// <summary>
        /// Gets the type to measure.
        /// </summary>
        /// <returns>The type to measure.</returns>
        public IType TypeToMeasure { get; private set; }

        /// <summary>
        /// Tells if the size of a reference to a reference type is measured,
        /// rather than its underlying size.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the size of a reference type's reference is returned;
        /// <c>false</c> if its underlying storage is measured.
        /// </returns>
        public bool IsReferenceSize { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Int32;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var type = BasicBlock.FunctionBody.Module.Declare(TypeToMeasure);
            if (!IsReferenceSize)
            {
                type = GetElementType(type);
            }

            return new BlockCodegen(BasicBlock, ConstTrunc(SizeOf(type), Int32Type()));
        }
    }
}