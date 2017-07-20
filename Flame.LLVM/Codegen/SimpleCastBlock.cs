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
            Func<LLVMBuilderRef, LLVMValueRef, LLVMTypeRef, string, LLVMValueRef> BuildCast,
            Func<LLVMValueRef, LLVMTypeRef, LLVMValueRef> BuildConstCast)
        {
            this.codeGen = CodeGenerator;
            this.Value = Value;
            this.targetType = Type;
            this.build = BuildCast;
            this.buildConst = BuildConstCast;
        }

        /// <summary>
        /// Gets the value to reinterpret.
        /// </summary>
        /// <returns>The value to reinterpret.</returns>
        public CodeBlock Value { get; private set; }

        private ICodeGenerator codeGen;
        private IType targetType;
        private Func<LLVMBuilderRef, LLVMValueRef, LLVMTypeRef, string, LLVMValueRef> build;
        private Func<LLVMValueRef, LLVMTypeRef, LLVMValueRef> buildConst;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => targetType;

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var valResult = Value.Emit(BasicBlock);
            BasicBlock = valResult.BasicBlock;
            var llvmType = BasicBlock.FunctionBody.Module.Declare(targetType);
            if (valResult.Value.IsConstant())
            {
                return new BlockCodegen(BasicBlock, buildConst(valResult.Value, llvmType));
            }
            else
            {
                return new BlockCodegen(
                    BasicBlock,
                    build(
                        BasicBlock.Builder,
                        valResult.Value,
                        llvmType,
                        "cast_tmp"));
            }
        }
    }
}