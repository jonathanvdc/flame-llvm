using System;
using System.Collections.Generic;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A block implementation that produces the default value of a struct.
    /// </summary>
    public sealed class DefaultStructBlock : CodeBlock
    {
        /// <summary>
        /// Creates a block that produces the default value of the given struct type.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Type">The struct value whose default value is to be created.</param>
        public DefaultStructBlock(LLVMCodeGenerator CodeGenerator, LLVMType Type)
        {
            this.codeGen = CodeGenerator;
            this.structType = Type;
        }

        private LLVMType structType;
        private LLVMCodeGenerator codeGen;
        
        public override ICodeGenerator CodeGenerator => codeGen;

        public override IType Type => structType;

        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            var elements = new LLVMValueRef[structType.InstanceFields.Count];
            for (int i = 0; i < elements.Length; i++)
            {
                var elemBlock = (CodeBlock)codeGen.EmitDefaultValue(structType.InstanceFields[i].FieldType);
                var elemResult = elemBlock.Emit(BasicBlock);
                BasicBlock = elemResult.BasicBlock;
                elements[i] = elemResult.Value;
            }
            return new BlockCodegen(BasicBlock, ConstStruct(elements, false));
        }
    }
}