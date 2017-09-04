using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that creates an unreachable path
    /// returning an undefined value.
    /// </summary>
    public sealed class UnreachableBlock : CodeBlock
    {
        /// <summary>
        /// Creates an unreachable block that produces a value of the given type.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates the block.</param>
        /// <param name="Type">The type of value that is created.</param>
        public UnreachableBlock(ICodeGenerator CodeGenerator, IType Type)
        {
            this.codeGen = CodeGenerator;
            this.resultType = Type;
        }

        private IType resultType;

        private ICodeGenerator codeGen;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => resultType;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            BuildUnreachable(BasicBlock.Builder);
            var newBlock = BasicBlock.CreateChildBlock("post_unreachable");
            return new BlockCodegen(
                newBlock,
                PrimitiveTypes.Void == Type
                    ? default(LLVMValueRef)
                    : GetUndef(BasicBlock.FunctionBody.Module.Declare(resultType)));
        }
    }
}