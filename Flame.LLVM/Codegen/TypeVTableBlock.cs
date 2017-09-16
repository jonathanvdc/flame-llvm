using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that produces the address of a type's vtable.
    /// </summary>
    public sealed class TypeVTableBlock : CodeBlock
    {
        /// <summary>
        /// Creates a code block that produces a pointer to the given type's vtable.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates the block.</param>
        /// <param name="VTableType">The type for which a vtable pointer is to be produced.</param>
        public TypeVTableBlock(
            LLVMCodeGenerator CodeGenerator,
            LLVMType VTableType)
        {
            this.codeGen = CodeGenerator;
            this.VTableType = VTableType;
        }

        /// <summary>
        /// Gets the type for which a vtable pointer is to be produced.
        /// </summary>
        /// <returns>The type for which a vtable pointer is to be produced.</returns>
        public LLVMType VTableType { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.UInt8.MakePointerType(PointerKind.TransientPointer);

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(
                BasicBlock,
                BuildTypeVTable(BasicBlock, VTableType));
        }

        /// <summary>
        /// Builds a value that computes a pointer to the vtable for the given type.
        /// </summary>
        /// <param name="BasicBlock">The basic block to build the instruction in.</param>
        /// <param name="Type">The type whose vtable pointer is to be computes.</param>
        /// <returns>A value that computes a vtable pointer.</returns>
        public static LLVMValueRef BuildTypeVTable(BasicBlockBuilder BasicBlock, IType Type)
        {
            return ConstBitCast(
                BasicBlock.FunctionBody.Module.GetVTable(Type).Pointer,
                PointerType(Int8Type(), 0));
        }
    }
}