using System;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class LLVMCodeGenerator : IUnmanagedCodeGenerator
    {
        public LLVMCodeGenerator(LLVMMethod Method)
        {
            this.owningMethod = Method;
            this.Prologue = new PrologueSpec();
            this.locals = new Dictionary<UniqueTag, TaggedValueBlock>();
            this.parameters = new List<TaggedValueBlock>();
            foreach (var param in Method.Parameters)
            {
                var alloca = new AllocaBlock(this, param.ParameterType);
                var storageTag = Prologue.AddInstruction(alloca);
                var taggedVal = new TaggedValueBlock(this, storageTag, alloca.Type);
                Prologue.AddInstruction(new StoreBlock(
                    this,
                    taggedVal,
                    new GetParameterBlock(this, parameters.Count, param.ParameterType)));
                parameters.Add(taggedVal);
            }
        }

        private LLVMMethod owningMethod;

        /// <summary>
        /// Gets the prologue for the method that defines this code generator.
        /// </summary>
        /// <returns>The prologue.</returns>
        public PrologueSpec Prologue { get; private set; }

        /// <summary>
        /// Gets the method that owns this code generator.
        /// </summary>
        public IMethod Method => owningMethod;

        private Dictionary<UniqueTag, TaggedValueBlock> locals;
        private List<TaggedValueBlock> parameters;

        private ICodeBlock EmitIntBinary(
            CodeBlock A,
            CodeBlock B,
            Operator Op,
            Dictionary<Operator, BuildLLVMBinary> BinaryBuilders,
            Dictionary<Operator, LLVMIntPredicate> BinaryPredicates)
        {
            LLVMIntPredicate pred;
            if (BinaryPredicates.TryGetValue(Op, out pred))
            {
                return new ComparisonBlock(this, A, B, pred);
            }
            else
            {
                return new BinaryBlock(this, A, B, A.Type, BinaryBuilders[Op]);
            }
        }

        public ICodeBlock EmitBinary(ICodeBlock A, ICodeBlock B, Operator Op)
        {
            var lhs = (CodeBlock)A;
            var rhs = (CodeBlock)B;
            var lhsType = lhs.Type;
            var rhsType = rhs.Type;
            if (lhsType.GetIsSignedInteger() && rhsType.GetIsSignedInteger())
            {
                return EmitIntBinary(lhs, rhs, Op, signedIntBinaries, signedIntPredicates);
            }
            else if (lhsType.GetIsUnsignedInteger() && rhsType.GetIsUnsignedInteger())
            {
                return EmitIntBinary(lhs, rhs, Op, unsignedIntBinaries, unsignedIntPredicates);
            }
            else if (lhsType.GetIsPointer() && rhsType.GetIsInteger())
            {
                if (Op.Equals(Operator.Add))
                {
                    return new GetElementPtrBlock(
                        this,
                        lhs,
                        new CodeBlock[] { rhs },
                        lhs.Type);
                }
                else if (Op.Equals(Operator.Subtract))
                {
                    return new GetElementPtrBlock(
                        this,
                        lhs,
                        new CodeBlock[] { (CodeBlock)EmitUnary(rhs, Operator.Subtract) },
                        lhs.Type);
                }
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

        private static readonly Dictionary<Operator, LLVMIntPredicate> signedIntPredicates =
            new Dictionary<Operator, LLVMIntPredicate>()
        {
            { Operator.CheckEquality, LLVMIntPredicate.LLVMIntEQ },
            { Operator.CheckInequality, LLVMIntPredicate.LLVMIntNE },
            { Operator.CheckGreaterThan, LLVMIntPredicate.LLVMIntSGT },
            { Operator.CheckGreaterThanOrEqual, LLVMIntPredicate.LLVMIntSGE },
            { Operator.CheckLessThan, LLVMIntPredicate.LLVMIntSLT },
            { Operator.CheckLessThanOrEqual, LLVMIntPredicate.LLVMIntSLE }
        };

        private static readonly Dictionary<Operator, BuildLLVMBinary> unsignedIntBinaries =
            new Dictionary<Operator, BuildLLVMBinary>()
        {
            { Operator.Add, BuildAdd },
            { Operator.Subtract, BuildSub },
            { Operator.Multiply, BuildMul },
            { Operator.Divide, BuildUDiv },
            { Operator.Remainder, BuildURem },
            { Operator.And, BuildAnd },
            { Operator.Or, BuildOr },
            { Operator.Xor, BuildXor },
            { Operator.LeftShift, BuildShl },
            { Operator.RightShift, BuildLShr }
        };

        private static readonly Dictionary<Operator, LLVMIntPredicate> unsignedIntPredicates =
            new Dictionary<Operator, LLVMIntPredicate>()
        {
            { Operator.CheckEquality, LLVMIntPredicate.LLVMIntEQ },
            { Operator.CheckInequality, LLVMIntPredicate.LLVMIntNE },
            { Operator.CheckGreaterThan, LLVMIntPredicate.LLVMIntUGT },
            { Operator.CheckGreaterThanOrEqual, LLVMIntPredicate.LLVMIntUGE },
            { Operator.CheckLessThan, LLVMIntPredicate.LLVMIntULT },
            { Operator.CheckLessThanOrEqual, LLVMIntPredicate.LLVMIntULE }
        };

        public ICodeBlock EmitTypeBinary(ICodeBlock Value, IType Type, Operator Op)
        {
            var valBlock = (CodeBlock)Value;
            if (Op.Equals(Operator.ReinterpretCast))
            {
                return new SimpleCastBlock(this, valBlock, Type, BuildPointerCast);
            }
            else if (Op.Equals(Operator.StaticCast))
            {
                var valType = valBlock.Type;
                if (valType.GetIsInteger() && Type.GetIsInteger())
                {
                    var valSpec = valType.GetIntegerSpec();
                    var targetSpec = Type.GetIntegerSpec();
                    if (valSpec.Size == targetSpec.Size)
                    {
                        return new RetypedBlock(this, valBlock, Type);
                    }
                    else if (valSpec.Size > targetSpec.Size)
                    {
                        return new SimpleCastBlock(this, valBlock, Type, BuildTrunc);
                    }
                    else if (targetSpec.IsSigned)
                    {
                        return new SimpleCastBlock(this, valBlock, Type, BuildSExt);
                    }
                    else
                    {
                        return new SimpleCastBlock(this, valBlock, Type, BuildZExt);
                    }
                }
                else if (valType.GetIsPointer() && Type.GetIsInteger())
                {
                    return new SimpleCastBlock(this, valBlock, Type, BuildPtrToInt);
                }
                else if (valType.GetIsInteger() && Type.GetIsPointer())
                {
                    return new SimpleCastBlock(this, valBlock, Type, BuildIntToPtr);
                }
            }
            throw new NotImplementedException();
        }

        public ICodeBlock EmitBit(BitValue Value)
        {
            var llvmType = IntType((uint)Value.Size);
            // TODO: support bit sizes greater than 64 bits
            var uint64Val = Value.ToInteger().Cast(IntegerSpec.UInt64).ToUInt64();
            return new ConstantBlock(
                this,
                PrimitiveTypes.GetBitType(Value.Size),
                ConstInt(llvmType, uint64Val, false));
        }

        public ICodeBlock EmitBoolean(bool Value)
        {
            return new ConstantBlock(
                this,
                PrimitiveTypes.Boolean,
                ConstInt(Int1Type(), Value ? 1ul : 0ul, false));
        }

        public ICodeBlock EmitChar(char Value)
        {
            return new ConstantBlock(
                this,
                PrimitiveTypes.Char,
                ConstInt(Int16Type(), Value, false));
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
            return new IfElseBlock(this, (CodeBlock)Condition, (CodeBlock)IfBody, (CodeBlock)ElseBody);
        }

        public ICodeBlock EmitTagged(UniqueTag Tag, ICodeBlock Contents)
        {
            return new TaggedFlowBlock(this, Tag, (CodeBlock)Contents);
        }

        public ICodeBlock EmitBreak(UniqueTag Target)
        {
            return new BranchBlock(this, Target, true);
        }

        public ICodeBlock EmitContinue(UniqueTag Target)
        {
            return new BranchBlock(this, Target, false);
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
            return new InvocationBlock(
                this,
                (CodeBlock)Method,
                Arguments.Cast<CodeBlock>());
        }

        public ICodeBlock EmitMethod(IMethod Method, ICodeBlock Caller, Operator Op)
        {
            return new DelegateBlock(this, Method, (CodeBlock)Caller, Op);
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
            return new ConstantBlock(
                this,
                PrimitiveTypes.Null,
                ConstNull(PointerType(IntType(8), 0)));
        }

        public ICodeBlock EmitPop(ICodeBlock Value)
        {
            return new PopBlock(this, (CodeBlock)Value);
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

        public ICodeBlock EmitUnary(ICodeBlock Value, Operator Op)
        {
            var valBlock = (CodeBlock)Value;
            var valType = valBlock.Type;
            if (Op.Equals(Operator.Not))
            {
                return new UnaryBlock(this, valBlock, valType, BuildNot);
            }
            else if (Op.Equals(Operator.Subtract))
            {
                if (valType.GetIsFloatingPoint())
                {
                    return new UnaryBlock(this, valBlock, valType, BuildFNeg);
                }
                else
                {
                    return new UnaryBlock(this, valBlock, valType, BuildNeg);
                }
            }
            throw new NotImplementedException();
        }

        public ICodeBlock EmitVoid()
        {
            return new VoidBlock(this);
        }

        public ICodeBlock EmitDereferencePointer(ICodeBlock Pointer)
        {
            return new AtAddressEmitVariable((CodeBlock)Pointer).EmitGet();
        }

        public ICodeBlock EmitStoreAtAddress(ICodeBlock Pointer, ICodeBlock Value)
        {
            return new AtAddressEmitVariable((CodeBlock)Pointer).EmitSet(Value);
        }

        public ICodeBlock EmitSizeOf(IType Type)
        {
            throw new NotImplementedException();
        }

        public IEmitVariable GetArgument(int Index)
        {
            return GetUnmanagedArgument(Index);
        }

        public IUnmanagedEmitVariable GetUnmanagedArgument(int Index)
        {
            return new AtAddressEmitVariable(parameters[Index]);
        }

        public IEmitVariable GetElement(ICodeBlock Value, IEnumerable<ICodeBlock> Index)
        {
            throw new NotImplementedException();
        }

        public IUnmanagedEmitVariable GetUnmanagedElement(ICodeBlock Value, IEnumerable<ICodeBlock> Index)
        {
            throw new NotImplementedException();
        }

        public IEmitVariable GetField(IField Field, ICodeBlock Target)
        {
            throw new NotImplementedException();
        }

        public IUnmanagedEmitVariable GetUnmanagedField(IField Field, ICodeBlock Target)
        {
            throw new NotImplementedException();
        }

        public IEmitVariable DeclareLocal(UniqueTag Tag, IVariableMember VariableMember)
        {
            return DeclareUnmanagedLocal(Tag, VariableMember);
        }

        public IUnmanagedEmitVariable DeclareUnmanagedLocal(UniqueTag Tag, IVariableMember VariableMember)
        {
            var alloca = new AllocaBlock(this, VariableMember.VariableType);
            var valueTag = Prologue.AddInstruction(alloca);
            var taggedValue = new TaggedValueBlock(this, valueTag, alloca.Type);
            locals.Add(Tag, taggedValue);
            return new AtAddressEmitVariable(taggedValue);
        }

        public IEmitVariable GetLocal(UniqueTag Tag)
        {
            return GetUnmanagedLocal(Tag);
        }

        public IUnmanagedEmitVariable GetUnmanagedLocal(UniqueTag Tag)
        {
            TaggedValueBlock address;
            if (locals.TryGetValue(Tag, out address))
            {
                return new AtAddressEmitVariable(address);
            }
            else
            {
                return null;
            }
        }

        public IEmitVariable GetThis()
        {
            throw new NotImplementedException();
        }
    }
}

