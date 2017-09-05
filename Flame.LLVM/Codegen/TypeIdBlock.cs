using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that produces a vtable's type ID.
    /// </summary>
    public sealed class TypeIdBlock : CodeBlock
    {
        /// <summary>
        /// Creates a code block that produces the given vtable's ID.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates the block.</param>
        /// <param name="VTablePointer">A pointer to the vtable whose type ID is to be retrieved.</param>
        public TypeIdBlock(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock VTablePointer)
        {
            this.codeGen = CodeGenerator;
            this.VTablePointer = VTablePointer;
        }

        /// <summary>
        /// Gets a pointer to the vtable whose type ID is to be retrieved.
        /// </summary>
        /// <returns>A pointer to the vtable whose type ID is to be retrieved.</returns>
        public CodeBlock VTablePointer { get; private set; }

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.UInt64;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var vtableCodegen = VTablePointer.Emit(BasicBlock);
            BasicBlock = vtableCodegen.BasicBlock;
            return new BlockCodegen(
                BasicBlock,
                BuildTypeid(BasicBlock.Builder, vtableCodegen.Value));
        }

        /// <summary>
        /// Creates a sequence of instructions that computes the type ID
        /// for the given VTable pointer.
        /// </summary>
        /// <param name="Builder">The builder to create the instructions in.</param>
        /// <param name="VTablePtr">A pointer to a VTable.</param>
        /// <returns>A type ID.</returns>
        public static LLVMValueRef BuildTypeid(
            LLVMBuilderRef Builder,
            LLVMValueRef VTablePtr)
        {
            return AtAddressEmitVariable.BuildConstantLoad(
                Builder,
                BuildStructGEP(
                    Builder,
                    BuildBitCast(
                        Builder,
                        VTablePtr,
                        PointerType(LLVMType.VTableType, 0),
                        "vtable_tmp"),
                    0,
                    "typeid_ptr_tmp"),
                "typeid_tmp");
        }
    }
}