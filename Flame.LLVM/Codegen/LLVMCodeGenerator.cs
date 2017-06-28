using System;
using System.Collections.Generic;
using Flame.Compiler;
using Flame.Compiler.Emit;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    using BuildLLVMBinary = Func<LLVMBuilderRef, LLVMValueRef, LLVMValueRef, string, LLVMValueRef>;

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

        public ICodeBlock EmitBinary(ICodeBlock A, ICodeBlock B, Operator Op)
        {
            var lhs = (CodeBlock)A;
            var rhs = (CodeBlock)B;
            if (lhs.Type.GetIsSignedInteger())
            {
                return new BinaryBlock(this, lhs, rhs, lhs.Type, signedIntBinaries[Op]);
            }
            throw new NotImplementedException();
        }

        private static readonly Dictionary<Operator, BuildLLVMBinary> signedIntBinaries =
            new Dictionary<Operator, BuildLLVMBinary>()
        {
            { Operator.Add, BuildAdd },
            { Operator.Subtract, BuildSub },
            { Operator.Multiply, BuildMul },
            { Operator.Divide, BuildSDiv },
            { Operator.Remainder, BuildSRem },
            { Operator.And, BuildAnd },
            { Operator.Or, BuildOr },
            { Operator.Xor, BuildXor },
            { Operator.LeftShift, BuildShl },
            { Operator.RightShift, BuildAShr }
        };

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
            return new ConstantBlock(
                this,
                PrimitiveTypes.Float32,
                ConstReal(FloatType(), Value));
        }

        public ICodeBlock EmitFloat64(double Value)
        {
            return new ConstantBlock(
                this,
                PrimitiveTypes.Float64,
                ConstReal(DoubleType(), Value));
        }

        public ICodeBlock EmitIfElse(ICodeBlock Condition, ICodeBlock IfBody, ICodeBlock ElseBody)
        {
            throw new NotImplementedException();
        }

        public ICodeBlock EmitInteger(IntegerValue Value)
        {
            var llvmType = IntType((uint)Value.Spec.Size);
            // TODO: support integer sizes greater than 64 bits
            var uint64Val = Value.Cast(IntegerSpec.UInt64).ToUInt64();
            return new ConstantBlock(
                this,
                PrimitiveTypes.GetIntegerType(Value.Spec.Size, Value.Spec.IsSigned),
                ConstInt(llvmType, uint64Val, Value.Spec.IsSigned));
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
            return new ReturnBlock(this, (CodeBlock)Value);
        }

        public ICodeBlock EmitSequence(ICodeBlock First, ICodeBlock Second)
        {
            return new SequenceBlock(this, (CodeBlock)First, (CodeBlock)Second);
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

        public IEmitVariable DeclareLocal(UniqueTag Tag, IVariableMember VariableMember)
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

