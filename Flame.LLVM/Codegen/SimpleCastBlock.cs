using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that applies a cast to a value.
    /// </summary>
    public sealed class SimpleCastBlock : CodeBlock
    {
        public SimpleCastBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Value,
            IType Type,
            Func<LLVMBuilderRef, LLVMValueRef, LLVMTypeRef, string, LLVMValueRef> BuildCast)
        {
            this.codeGen = CodeGenerator;
            this.Value = Value;
            this.targetType = Type;
            this.build = BuildCast;
        }

        /// <summary>
        /// Gets the value to reinterpret.
        /// </summary>
        /// <returns>The value to reinterpret.</returns>
        public CodeBlock Value { get; private set; }

        private ICodeGenerator codeGen;
        private IType targetType;
        private Func<LLVMBuilderRef, LLVMValueRef, LLVMTypeRef, string, LLVMValueRef> build;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => targetType;

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var valResult = Value.Emit(BasicBlock);
            BasicBlock = valResult.BasicBlock;
            return new BlockCodegen(
                BasicBlock,
                build(
                    BasicBlock.Builder,
                    valResult.Value,
                    BasicBlock.FunctionBody.Module.Declare(targetType),
                    "cast_tmp"));
        }
    }
}