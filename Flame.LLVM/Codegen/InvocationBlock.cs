using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A code block that invokes a delegate.
    /// </summary>
    public sealed class InvocationBlock : CodeBlock
    {
        /// <summary>
        /// Creates an invocation block from a callee and an argument list.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="Callee">The delegate to call.</param>
        /// <param name="Arguments">The argument list for the call.</param>
        /// <param name="CanThrow">Tells if the invocation can throw an exception.</param>
        public InvocationBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Callee,
            IEnumerable<CodeBlock> Arguments,
            bool CanThrow)
        {
            this.codeGen = CodeGenerator;
            this.Callee = Callee;
            this.Arguments = Arguments;
            this.methodSignature = MethodType.GetMethod(Callee.Type);
            this.retType = methodSignature.ReturnType;
            this.CanThrow = CanThrow;
        }

        /// <summary>
        /// Gets a block that produces the delegate to invoke.
        /// </summary>
        /// <returns>The delegate to invoke.</returns>
        public CodeBlock Callee { get; private set; }

        /// <summary>
        /// Gets the argument list for the call.
        /// </summary>
        /// <returns>The argument list.</returns>
        public IEnumerable<CodeBlock> Arguments { get; private set; }

        /// <summary>
        /// Tells if the invocation can throw an exception.
        /// </summary>
        /// <returns><c>true</c> if the invocation can throw; otherwise, <c>false</c>.</returns>
        public bool CanThrow { get; private set; }

        private ICodeGenerator codeGen;

        private IType retType;

        private IMethod methodSignature;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => retType;

        private Tuple<LLVMValueRef[], BasicBlockBuilder> EmitArguments(
            BasicBlockBuilder BasicBlock,
            LLVMValueRef Target,
            IEnumerable<CodeBlock> Arguments)
        {
            int targetArgCount = Target.Pointer == IntPtr.Zero ? 0 : 1;
            var argArr = Arguments.ToArray<CodeBlock>();
            var allArgs = new LLVMValueRef[targetArgCount + argArr.Length];
            if (targetArgCount == 1)
            {
                allArgs[0] = BuildBitCast(
                    BasicBlock.Builder,
                    Target,
                    PointerType(Int8Type(), 0),
                    "this_tmp");
            }
            for (int i = 0; i < argArr.Length; i++)
            {
                var argResult = argArr[i].Emit(BasicBlock);
                BasicBlock = argResult.BasicBlock;
                allArgs[targetArgCount + i] = argResult.Value;
            }
            return new Tuple<LLVMValueRef[], BasicBlockBuilder>(
                allArgs, BasicBlock);
        }

        /// <summary>
        /// Generates code for the object that receives a call, if any.
        /// </summary>
        /// <param name="BasicBlock">The basic block to generate code in.</param>
        /// <param name="Target">An object that will receive a call or <c>null</c>.</param>
        /// <returns>Block codegen.</returns>
        public static BlockCodegen EmitTarget(BasicBlockBuilder BasicBlock, CodeBlock Target)
        {
            if (Target == null)
            {
                return new BlockCodegen(BasicBlock);
            }
            else
            {
                var targetResult = Target.Emit(BasicBlock);
                BasicBlock = targetResult.BasicBlock;
                return new BlockCodegen(BasicBlock, targetResult.Value);
            }
        }

        /// <summary>
        /// Generates code that computes the address of the given callee function's
        /// implementation.
        /// </summary>
        /// <param name="BasicBlock">The basic block to generate code in.</param>
        /// <param name="Target">The object that receives the call.</param>
        /// <param name="Callee">The callee method.</param>
        /// <param name="Op">The call operator.</param>
        /// <returns>A function pointer.</returns>
        public static BlockCodegen EmitCallee(
            BasicBlockBuilder BasicBlock,
            LLVMValueRef Target,
            IMethod Callee,
            Operator Op)
        {
            var module = BasicBlock.FunctionBody.Module;
            if (Op.Equals(Operator.GetDelegate)
                || Op.Equals(Operator.GetCurriedDelegate))
            {
                return new BlockCodegen(BasicBlock, module.Declare(Callee));
            }
            else if (Op.Equals(Operator.GetVirtualDelegate))
            {
                var method = (LLVMMethod)Callee;

                if (method.DeclaringType.GetIsValueType()
                    || !method.GetIsVirtual())
                {
                    return EmitCallee(BasicBlock, Target, Callee, Operator.GetDelegate);
                }

                var vtablePtr = BuildBitCast(
                    BasicBlock.Builder,
                    Target,
                    PointerType(PointerType(LLVMType.VTableType, 0), 0),
                    "vtable_ptr_tmp");

                var vtable = AtAddressEmitVariable.BuildConstantLoad(
                    BasicBlock.Builder,
                    vtablePtr,
                    "vtable_tmp");

                if (method.DeclaringType.GetIsInterface())
                {
                    // Resolve interface method implementations by calling a stub.
                    var typeIndexPtr = BuildStructGEP(
                        BasicBlock.Builder,
                        vtable,
                        1,
                        "type_index_ptr");

                    var typeIndex = AtAddressEmitVariable.BuildConstantLoad(
                        BasicBlock.Builder,
                        typeIndexPtr,
                        "type_ptr");

                    var stub = module.GetInterfaceStub(method);

                    var methodImpl = BuildCall(
                        BasicBlock.Builder,
                        stub.Function,
                        new LLVMValueRef[] { typeIndex },
                        "iface_method_ptr");

                    return new BlockCodegen(BasicBlock, methodImpl);
                }
                else
                {
                    // Resolve virtual/abstract method implementations by performing
                    // a table lookup.
                    var vtableSlot = module
                        .GetVTable(method.ParentType)
                        .GetAbsoluteSlot(method);

                    var vtableContentPtr = BuildBitCast(
                        BasicBlock.Builder,
                        BuildStructGEP(
                            BasicBlock.Builder,
                            vtable,
                            2,
                            "vtable_method_array_ptr"),
                        PointerType(PointerType(Int8Type(), 0), 0),
                        "vtable_methods_ptr");

                    var vtableSlotPtr = BuildGEP(
                        BasicBlock.Builder,
                        vtableContentPtr,
                        new LLVMValueRef[] { ConstInt(Int32Type(), (ulong)vtableSlot, false) },
                        "vtable_slot_ptr");

                    var methodImpl = AtAddressEmitVariable.BuildConstantLoad(
                        BasicBlock.Builder,
                        BuildBitCast(
                            BasicBlock.Builder,
                            vtableSlotPtr,
                            PointerType(PointerType(module.DeclarePrototype(method), 0), 0),
                            "method_ptr_field"
                        ),
                        "method_ptr");

                    return new BlockCodegen(BasicBlock, methodImpl);
                }
            }
            else
            {
                throw new NotImplementedException(
                    string.Format("Unsupported call operator: {0}.", Op.Name));
            }
        }

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            if (Callee is DelegateBlock)
            {
                var deleg = (DelegateBlock)Callee;

                var targetAndBlock = EmitTarget(BasicBlock, deleg.Target);
                BasicBlock = targetAndBlock.BasicBlock;

                var argsAndBlock = EmitArguments(BasicBlock, targetAndBlock.Value, Arguments);
                BasicBlock = argsAndBlock.Item2;

                var calleeAndBlock = EmitCallee(BasicBlock, targetAndBlock.Value, deleg.Callee, deleg.Op);
                BasicBlock = calleeAndBlock.BasicBlock;

                return EmitCall(BasicBlock, calleeAndBlock.Value, argsAndBlock.Item1);
            }
            else if (Callee is IntrinsicBlock)
            {
                var argsAndBlock = EmitArguments(BasicBlock, new LLVMValueRef(IntPtr.Zero), Arguments);
                BasicBlock = argsAndBlock.Item2;

                var intrinsicAndBlock = Callee.Emit(BasicBlock);
                BasicBlock = intrinsicAndBlock.BasicBlock;

                return EmitCall(BasicBlock, intrinsicAndBlock.Value, argsAndBlock.Item1);
            }
            else
            {
                // To invoke a delegate, we first need to figure out if it includes
                // a context or not. If so, then we should prepend a context value
                // to the argument list.
                //
                // The code'll look more or less like this:
                //
                //     var delegate = ...;
                //     var args... = ...;
                //     void* fptr = delegate->fptr;
                //     if (delegate->has_context)
                //     {
                //         void* context = delegate->context;
                //         result1 = ((TRet(void*, TArgs...)*)fptr)(context, args...);
                //     }
                //     else
                //     {
                //         result2 = ((TRet(TArgs...)*)fptr)(args...);
                //     }
                //     result = phi(result1, result2);

                var delegateAndBlock = Callee.Emit(BasicBlock);
                BasicBlock = delegateAndBlock.BasicBlock;

                var argsAndBlock = EmitArguments(BasicBlock, default(LLVMValueRef), Arguments);
                BasicBlock = argsAndBlock.Item2;

                var contextfulCallBlock = BasicBlock.CreateChildBlock("contextful_call");
                var contextlessCallBlock = BasicBlock.CreateChildBlock("contextless_call");
                var postCallBlock = BasicBlock.CreateChildBlock("post_call");

                var funcPrototype = BasicBlock.FunctionBody.Module.DeclarePrototype(
                    methodSignature);

                var contextFuncPrototype =
                    methodSignature.IsStatic
                    ? FunctionType(
                        funcPrototype.GetReturnType(),
                        new LLVMTypeRef[] { PointerType(Int8Type(), 0) }
                            .Concat<LLVMTypeRef>(funcPrototype.GetParamTypes())
                            .ToArray<LLVMTypeRef>(),
                        funcPrototype.IsFunctionVarArg)
                    : funcPrototype;

                var contextlessFuncPrototype =
                    methodSignature.IsStatic
                    ? funcPrototype
                    : FunctionType(
                        funcPrototype.GetReturnType(),
                        funcPrototype.GetParamTypes()
                            .Skip<LLVMTypeRef>(1)
                            .ToArray<LLVMTypeRef>(),
                        funcPrototype.IsFunctionVarArg);

                var delegatePtr = BuildBitCast(
                    BasicBlock.Builder,
                    delegateAndBlock.Value,
                    PointerType(DelegateBlock.MethodTypeLayout, 0),
                    "delegate_ptr");

                var funcPtr = DelegateBlock.BuildLoadFunctionPointer(
                    BasicBlock.Builder, delegatePtr);

                var hasContext = DelegateBlock.BuildLoadHasContext(
                    BasicBlock.Builder, delegatePtr);

                BuildCondBr(
                    BasicBlock.Builder,
                    hasContext,
                    contextfulCallBlock.Block,
                    contextlessCallBlock.Block);

                // Write the contextful call block.
                var contextPtr = DelegateBlock.BuildLoadContextObject(
                    contextfulCallBlock.Builder, delegatePtr);

                var contextfulCallAndBlock = EmitCall(
                    contextfulCallBlock,
                    BuildBitCast(
                        contextfulCallBlock.Builder,
                        funcPtr,
                        PointerType(contextFuncPrototype, 0),
                        "contextful_fptr"),
                    new LLVMValueRef[] { contextPtr }
                        .Concat<LLVMValueRef>(argsAndBlock.Item1)
                        .ToArray<LLVMValueRef>());
                contextfulCallBlock = contextfulCallAndBlock.BasicBlock;

                BuildBr(contextfulCallBlock.Builder, postCallBlock.Block);

                // Write the contextless call block.
                var contextlessCallAndBlock = EmitCall(
                    contextlessCallBlock,
                    BuildBitCast(
                        contextlessCallBlock.Builder,
                        funcPtr,
                        PointerType(contextlessFuncPrototype, 0),
                        "contextless_fptr"),
                    argsAndBlock.Item1);
                contextlessCallBlock = contextlessCallAndBlock.BasicBlock;

                BuildBr(contextlessCallBlock.Builder, postCallBlock.Block);

                // Write a phi if the return type is non-void.
                if (contextfulCallAndBlock.HasValue)
                {
                    var resultPhi = BuildPhi(
                        postCallBlock.Builder,
                        contextfulCallAndBlock.Value.TypeOf(),
                        "result_phi");
                    resultPhi.AddIncoming(
                        new LLVMValueRef[]
                        {
                            contextfulCallAndBlock.Value,
                            contextlessCallAndBlock.Value
                        },
                        new LLVMBasicBlockRef[]
                        {
                            contextfulCallAndBlock.BasicBlock.Block,
                            contextlessCallAndBlock.BasicBlock.Block
                        },
                        2);
                    return new BlockCodegen(postCallBlock, resultPhi);
                }
                else
                {
                    return new BlockCodegen(postCallBlock);
                }
            }
        }

        private BlockCodegen EmitCall(
            BasicBlockBuilder BasicBlock,
            LLVMValueRef Callee,
            LLVMValueRef[] Arguments)
        {
            bool hasVoidRetType = retType == PrimitiveTypes.Void;

            LLVMValueRef callRef;
            if (CanThrow && BasicBlock.HasUnwindTarget)
            {
                var successBlock = BasicBlock.CreateChildBlock("success");
                callRef = BuildInvoke(
                    BasicBlock.Builder,
                    Callee,
                    Arguments,
                    successBlock.Block,
                    BasicBlock.UnwindTarget,
                    hasVoidRetType ? "" : "call_tmp");
                BasicBlock = successBlock;
            }
            else
            {
                callRef = BuildCall(
                    BasicBlock.Builder,
                    Callee,
                    Arguments,
                    hasVoidRetType ? "" : "call_tmp");
            }

            return hasVoidRetType
                ? new BlockCodegen(BasicBlock)
                : new BlockCodegen(BasicBlock, callRef);
        }
    }
}