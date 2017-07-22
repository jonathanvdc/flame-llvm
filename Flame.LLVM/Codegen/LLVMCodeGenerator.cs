using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Compiler;
using Flame.Compiler.Emit;
using Flame.Compiler.Expressions;
using Flame.Compiler.Statements;
using Flame.Compiler.Variables;
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
        public LLVMCodeGenerator(IMethod Method)
        {
            this.owningMethod = Method;
            this.Prologue = new PrologueSpec();
            this.locals = new Dictionary<UniqueTag, TaggedValueBlock>();
            this.parameters = new List<TaggedValueBlock>();
            if (Method != null)
            {
                // `Method` is null when we're generating constant field initializers.
                int thisParamCount = Method.IsStatic ? 0 : 1;
                if (!Method.IsStatic)
                {
                    thisParameter = SpillParameter(ThisVariable.GetThisType(Method.DeclaringType), 0);
                }
                foreach (var param in Method.Parameters)
                {
                    parameters.Add(SpillParameter(param.ParameterType, thisParamCount + parameters.Count));
                }
            }
        }

        private IMethod owningMethod;

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
        private TaggedValueBlock thisParameter;

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
                return new SimpleCastBlock(this, valBlock, Type, BuildPointerCast, ConstPointerCast);
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
                        return new SimpleCastBlock(this, valBlock, Type, BuildTrunc, ConstTrunc);
                    }
                    else if (valSpec.IsSigned)
                    {
                        return new SimpleCastBlock(this, valBlock, Type, BuildSExt, ConstSExt);
                    }
                    else
                    {
                        return new SimpleCastBlock(this, valBlock, Type, BuildZExt, ConstZExt);
                    }
                }
                else if (valType == PrimitiveTypes.Char)
                {
                    return EmitTypeBinary(new RetypedBlock(this, valBlock, PrimitiveTypes.Int16), Type, Op);
                }
                else if (Type == PrimitiveTypes.Char)
                {
                    return new RetypedBlock(this, (CodeBlock)EmitTypeBinary(valBlock, PrimitiveTypes.Int16, Op), Type);
                }
                else if (valType.GetIsPointer() && Type.GetIsInteger())
                {
                    return new SimpleCastBlock(this, valBlock, Type, BuildPtrToInt, ConstPtrToInt);
                }
                else if (valType.GetIsInteger() && Type.GetIsPointer())
                {
                    return new SimpleCastBlock(this, valBlock, Type, BuildIntToPtr, ConstIntToPtr);
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
            if (Type.GetIsInteger() || Type.GetIsFloatingPoint())
            {
                return new StaticCastExpression(new IntegerExpression(0), Type)
                    .Simplify()
                    .Emit(this);
            }
            else if (Type == PrimitiveTypes.Char)
            {
                return EmitChar(default(char));
            }
            else if (Type == PrimitiveTypes.Boolean)
            {
                return EmitBoolean(default(bool));
            }
            else if (Type.GetIsPointer())
            {
                return EmitTypeBinary(EmitNull(), Type, Operator.ReinterpretCast);
            }
            else if (Type.GetIsValueType())
            {
                return new DefaultStructBlock(this, (LLVMType)Type);
            }
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

        public ICodeBlock EmitProduct(IEnumerable<ICodeBlock> Values)
        {
            var accumulator = EmitInteger(new IntegerValue(1));
            foreach (var dim in Values)
            {
                accumulator = EmitBinary(
                    accumulator,
                    dim,
                    Operator.Multiply);
            }
            return accumulator;
        }

        private IReadOnlyList<ICodeBlock> SpillDimensions(
            IEnumerable<ICodeBlock> Dimensions,
            List<IStatement> Statements)
        {
            var dimVals = new List<ICodeBlock>();
            foreach (var dim in Dimensions)
            {
                var dimTmp = new SSAVariable("dimension_tmp", PrimitiveTypes.Int32);
                Statements.Add(
                    dimTmp.CreateSetStatement(
                        ToExpression(
                            (CodeBlock)EmitTypeBinary(dim, PrimitiveTypes.Int32, Operator.StaticCast))));
                dimVals.Add(dimTmp.CreateGetExpression().Emit(this));
            }
            return dimVals;
        }

        private void StoreDimensionsInArrayHeader(
            CodeBlock ArrayPointer,
            IReadOnlyList<ICodeBlock> Dimensions,
            List<IStatement> Statements)
        {
            for (int i = 0; i < Dimensions.Count; i++)
            {
                Statements.Add(
                    new CodeBlockStatement(
                        EmitStoreAtAddress(
                            new GetDimensionPtrBlock(this, ArrayPointer, i),
                            Dimensions[i])));
            }
        }

        public ICodeBlock EmitNewArray(IType ElementType, IEnumerable<ICodeBlock> Dimensions)
        {
            var arrayType = ElementType.MakeArrayType(Enumerable.Count<ICodeBlock>(Dimensions));

            // First, store all dimensions in temporaries.
            var statements = new List<IStatement>();
            var dimVals = SpillDimensions(Dimensions, statements);
            var arrayTmp = new SSAVariable("array_tmp", arrayType);

            // The number of elements to allocate is equal to the product of
            // the dimensions.
            var elemCount = EmitProduct(dimVals);

            // The size of the chunk of memory to allocate is
            //
            //     sizeof({ i32, ..., [0 x <element type>]}) + elemCount * sizeof(<element type>)
            //
            var allocationSize = EmitBinary(
                new SizeOfBlock(this, arrayType, false),
                EmitBinary(
                    elemCount,
                    new SizeOfBlock(this, ElementType, true),
                    Operator.Multiply),
                Operator.Add);

            // Allocate the array and store it in a temporary.
            statements.Add(
                arrayTmp.CreateSetStatement(
                    Allocate(
                        new CodeBlockExpression(allocationSize, PrimitiveTypes.Int32),
                        arrayType)));

            // Store the array's dimensions in the array header.
            StoreDimensionsInArrayHeader(
                (CodeBlock)arrayTmp.CreateGetExpression().Emit(this),
                dimVals,
                statements);

            return EmitSequence(
                new BlockStatement(statements).Emit(this),
                arrayTmp.CreateGetExpression().Emit(this));
        }

        private static IExpression ToExpression(CodeBlock Block)
        {
            return new CodeBlockExpression(Block, Block.Type);
        }

        private IExpression Allocate(IExpression Size, IType ResultType)
        {
            return new ReinterpretCastExpression(
                ((LLVMMethod)Method).Abi.GarbageCollector.Allocate(
                    new StaticCastExpression(Size, PrimitiveTypes.UInt64)),
                ResultType);
        }

        public ICodeBlock EmitNewObject(IMethod Constructor, IEnumerable<ICodeBlock> Arguments)
        {
            var constructedType = Constructor.DeclaringType;
            if (constructedType.GetIsValueType())
            {
                throw new InvalidOperationException(
                    "cannot create a new 'struct' object; " +
                    "'struct' object creation must be lowered to " +
                    "method invocation before codegen");
            }
            else if (constructedType.GetIsReferenceType())
            {
                var tmp = new SSAVariable("class_tmp", constructedType);
                // Write the following code:
                //
                //     var ptr = gcalloc(sizeof(T));
                //     ptr.ctor(args...);
                //     ptr
                //
                var expr = new InitializedExpression(
                    new BlockStatement(new IStatement[]
                    {
                        tmp.CreateSetStatement(
                            Allocate(
                                ToExpression(new SizeOfBlock(this, constructedType, false)),
                                constructedType)),
                        new ExpressionStatement(
                            new InvocationExpression(
                                Constructor,
                                tmp.CreateGetExpression(),
                                Arguments.Cast<CodeBlock>()
                                .Select<CodeBlock, IExpression>(ToExpression)
                                .ToArray<IExpression>()))
                    }),
                    tmp.CreateGetExpression());
                return expr.Emit(this);
            }
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
                return new UnaryBlock(this, valBlock, valType, BuildNot, ConstNot);
            }
            else if (Op.Equals(Operator.Subtract))
            {
                if (valType.GetIsFloatingPoint())
                {
                    return new UnaryBlock(this, valBlock, valType, BuildFNeg, ConstFNeg);
                }
                else
                {
                    return new UnaryBlock(this, valBlock, valType, BuildNeg, ConstNeg);
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
            return new SizeOfBlock(this, Type);
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
            return GetUnmanagedElement(Value, Index);
        }

        public IUnmanagedEmitVariable GetUnmanagedElement(ICodeBlock Value, IEnumerable<ICodeBlock> Index)
        {
            var valBlock = (CodeBlock)Value;
            var valType = valBlock.Type;
            if (valType.GetIsArray())
            {
                // Suppose that an n-dimensional array with dimensions
                //
                //     dim_1, dim_2, ..., dim_n
                //
                // is indexed with
                //
                //     i_1, i_2, ..., i_n.
                //
                // To compute a pointer to the element with that index, we can
                // use the following formula:
                //
                //     offset = i_1 + i_2 * dim_1 + i_3 * dim_1 * dim_2 + ... +
                //              i_n * dim_1 * dim_2 * ... * dim_n-1
                //
                //            = i_1 + dim_1 * (i_2 + dim_2 * (...))
                //
                // The latter identity requires fewer multiplications, so we'll use that.

                var arrayPtrTmp = new SSAVariable("array_tmp", valBlock.Type);
                var arrayPtr = (CodeBlock)arrayPtrTmp.CreateGetExpression().Emit(this);
                var indexArray = Index.ToArray<ICodeBlock>();
                var offset = EmitInteger(new IntegerValue(0));
                for (int i = indexArray.Length - 1; i >= 0; i--)
                {
                    // offset <- i_i + dim_i * offset
                    offset = EmitBinary(
                        EmitTypeBinary(indexArray[i], PrimitiveTypes.Int32, Operator.StaticCast),
                        EmitBinary(
                            EmitDereferencePointer(new GetDimensionPtrBlock(this, arrayPtr, i)),
                            offset,
                            Operator.Multiply),
                        Operator.Add);
                }

                // Now add this offset to the data pointer of the array and dereference the result.
                return new AtAddressEmitVariable(
                    (CodeBlock)EmitSequence(
                        arrayPtrTmp.CreateSetStatement(ToExpression(valBlock)).Emit(this),
                        EmitBinary(new GetDataPtrBlock(this, arrayPtr), offset, Operator.Add)));
            }

            throw new NotImplementedException();
        }

        public IEmitVariable GetField(IField Field, ICodeBlock Target)
        {
            return GetUnmanagedField(Field, Target);
        }

        public IUnmanagedEmitVariable GetUnmanagedField(IField Field, ICodeBlock Target)
        {
            if (Field.IsStatic)
            {
                return new AtAddressEmitVariable(new GetFieldPtrBlock(this, (LLVMField)Field));
            }

            var targetBlock = (CodeBlock)Target;
            if (targetBlock.Type.GetIsValueType())
            {
                // Spill by-value structs to a local first.
                targetBlock = SpillToTempAddress(targetBlock);
            }
            return new AtAddressEmitVariable(new GetFieldPtrBlock(this, targetBlock, (LLVMField)Field));
        }

        private void SpillToAddress(CodeBlock Value, out TaggedValueBlock Address, out CodeBlock Store)
        {
            var alloca = new AllocaBlock(this, Value.Type);
            var storageTag = Prologue.AddInstruction(alloca);
            Address = new TaggedValueBlock(this, storageTag, alloca.Type);
            Store = new StoreBlock(this, Address, Value);
        }

        private TaggedValueBlock SpillParameter(IType Type, int ExtendedParameterIndex)
        {
            CodeBlock store;
            TaggedValueBlock ptr;
            SpillToAddress(new GetParameterBlock(this, ExtendedParameterIndex, Type), out ptr, out store);
            Prologue.AddInstruction(store);
            return ptr;
        }

        private CodeBlock SpillToTempAddress(CodeBlock Value)
        {
            CodeBlock store;
            TaggedValueBlock ptr;
            SpillToAddress(Value, out ptr, out store);
            return (CodeBlock)EmitSequence(store, ptr);
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
            return new AtAddressEmitVariable(thisParameter);
        }
    }
}

