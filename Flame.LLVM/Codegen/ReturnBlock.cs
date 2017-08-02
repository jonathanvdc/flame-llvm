using System;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block implementation that returns a value from a method.
    /// </summary>
    public sealed class ReturnBlock : CodeBlock
    {
        public ReturnBlock(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock ReturnValue)
        {
            this.codeGen = CodeGenerator;
            this.retVal = ReturnValue;
        }

        private LLVMCodeGenerator codeGen;
        private CodeBlock retVal;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => PrimitiveTypes.Void;

        private BlockCodegen EmitRetVoid(BasicBlockBuilder BasicBlock)
        {
            var retVoid = BuildRetVoid(BasicBlock.Builder);
            return new BlockCodegen(BasicBlock);
        }

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            if (retVal == null)
            {
                return EmitRetVoid(BasicBlock);
            }

            var retValCodegen = retVal.Emit(BasicBlock);
            BasicBlock = retValCodegen.BasicBlock;
            if (retVal.Type == PrimitiveTypes.Void)
            {
                return EmitRetVoid(BasicBlock);
            }
            else
            {
                var ret = BuildRet(BasicBlock.Builder, retValCodegen.Value);
                return new BlockCodegen(BasicBlock);
            }
        }
    }
}

