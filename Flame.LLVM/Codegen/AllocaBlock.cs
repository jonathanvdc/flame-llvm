using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that allocates space for a single object on the stack.
    /// </summary>
    public sealed class AllocaBlock : CodeBlock
    {
        public AllocaBlock(ICodeGenerator CodeGenerator, IType ElementType)
        {
            this.codeGen = CodeGenerator;
            this.ElementType = ElementType;
        }

        private ICodeGenerator codeGen;

        /// <summary>
        /// Gets the type of value for which storage is allocated.
        /// </summary>
        /// <returns>The element type.</returns>
        public IType ElementType { get; private set; }

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => ElementType.MakePointerType(PointerKind.TransientPointer);

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            return new BlockCodegen(
                BasicBlock,
                BuildAlloca(BasicBlock.Builder, BasicBlock.FunctionBody.Module.Declare(ElementType),
                "alloca_tmp"));
        }
    }
}