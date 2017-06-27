using System;
using System.Collections.Generic;
using Flame.Compiler;
using Flame.Compiler.Emit;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code generator implementation that generates LLVM IR.
    /// </summary>
    public sealed class LLVMCodeGenerator : ICodeGenerator
    {
        public LLVMCodeGenerator(LLVMMethod Method)
        {
            this.owningMethod = Method;
        }

        private LLVMMethod owningMethod;

        /// <summary>
        /// Gets the method that owns this code generator.
        /// </summary>
        public IMethod Method => owningMethod;

        public IEmitVariable DeclareLocal(UniqueTag Tag, IVariableMember VariableMember)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitBinary(ICodeBlock A, ICodeBlock B, Operator Op)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitBit(BitValue Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitBoolean(bool Value)
        {
            return new ConstantBlock(
                this,
                PrimitiveTypes.Boolean,
                ConstInt(Int1Type(), Value ? 1ul : 0ul, false));
        }

        public ICodeBlock EmitBreak(UniqueTag Target)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitChar(char Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitContinue(UniqueTag Target)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitDefaultValue(IType Type)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitFloat32(float Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitFloat64(double Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitIfElse(ICodeBlock Condition, ICodeBlock IfBody, ICodeBlock ElseBody)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitInteger(IntegerValue Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitInvocation(ICodeBlock Method, IEnumerable<ICodeBlock> Arguments)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitMethod(IMethod Method, ICodeBlock Caller, Operator Op)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitNewArray(IType ElementType, IEnumerable<ICodeBlock> Dimensions)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitNewObject(IMethod Constructor, IEnumerable<ICodeBlock> Arguments)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitNewVector(IType ElementType, IReadOnlyList<int> Dimensions)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitNull()
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitPop(ICodeBlock Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitReturn(ICodeBlock Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitSequence(ICodeBlock First, ICodeBlock Second)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitString(string Value)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitTagged(UniqueTag Tag, ICodeBlock Contents)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitTypeBinary(ICodeBlock Value, IType Type, Operator Op)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitUnary(ICodeBlock Value, Operator Op)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitVoid()
        {
            throw new NotImplementedException();
        }

        public IEmitVariable GetArgument(int Index)
        {
            throw new NotImplementedException();
        }

        public IEmitVariable GetElement(ICodeBlock Value, IEnumerable<ICodeBlock> Index)
        {
            throw new NotImplementedException();
        }

        public IEmitVariable GetField(IField Field, ICodeBlock Target)
        {
            throw new NotImplementedException();
        }

        public IEmitVariable GetLocal(UniqueTag Tag)
        {
            throw new NotImplementedException();
        }

        public IEmitVariable GetThis()
        {
            throw new NotImplementedException();
        }
    }
}

