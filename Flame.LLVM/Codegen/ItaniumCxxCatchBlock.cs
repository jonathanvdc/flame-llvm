using System.Collections.Generic;
using Flame.Compiler;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// A try-catch block implementation for the Itanium C++ ABI.
    /// </summary>
    public sealed class ItaniumCxxCatchBlock : CodeBlock
    {
        /// <summary>
        /// Creates a try-catch block.
        /// </summary>
        /// <param name="CodeGenerator">The code generator that creates this block.</param>
        /// <param name="TryBody">The body of the try clause.</param>
        /// <param name="CatchClauses">The list of catch clauses.</param>
        public ItaniumCxxCatchBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock TryBody,
            IReadOnlyList<CatchClause> CatchClauses)
        {
            this.codeGenerator = CodeGenerator;
            this.TryBody = TryBody;
            this.CatchClauses = CatchClauses;
        }

        /// <summary>
        /// Gets the body of the try clause in this block.
        /// </summary>
        /// <returns>The try clause's body.</returns>
        public CodeBlock TryBody { get; private set; }

        /// <summary>
        /// Gets a read-only list of catch clauses in this block.
        /// </summary>
        /// <returns>The list of catch clauses.</returns>
        public IReadOnlyList<CatchClause> CatchClauses { get; private set; }

        private ICodeGenerator codeGenerator;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGenerator;

        /// <inheritdoc/>
        public override IType Type => TryBody.Type;

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            if (CatchClauses.Count == 0)
            {
                return TryBody.Emit(BasicBlock);
            }

            var exceptionDataType = StructType(new[] { PointerType(Int8Type(), 0), Int32Type() }, false);

            var catchBlock = BasicBlock.CreateChildBlock("catch");
            var catchLandingPadBlock = BasicBlock.CreateChildBlock("catch_landingpad");
            var leaveBlock = BasicBlock.CreateChildBlock("leave");

            // The try block is a regular block that jumps to the 'leave' block.
            //
            // try:
            //     <try body>
            //     br label %leave

            var tryCodegen = TryBody.Emit(BasicBlock.WithUnwindTarget(catchLandingPadBlock, catchBlock));
            BuildBr(tryCodegen.BasicBlock.Builder, leaveBlock.Block);

            PopulateCatchBlock(catchBlock, leaveBlock);
            ItaniumCxxFinallyBlock.PopulateThunkLandingPadBlock(catchLandingPadBlock, catchBlock.Block, false);

            return new BlockCodegen(leaveBlock, tryCodegen.Value);
        }

        /// <summary>
        /// Populates the given 'catch' block.
        /// </summary>
        /// <param name="CatchBlock">The 'catch' block to populate.</param>
        /// <param name="LeaveBlock">The 'leave' block to jump to when the 'catch' is done.</param>
        private void PopulateCatchBlock(BasicBlockBuilder CatchBlock, BasicBlockBuilder LeaveBlock)
        {
            var catchBlockEntry = CatchBlock;

            // Before we even get started on the catch block's body, we should first
            // take the time to ensure that the '__cxa_begin_catch' environment set up
            // by the catch block is properly terminated by a '__cxa_end_catch' call,
            // even if an exception is thrown from the catch block itself. We can do
            // so by creating a 'catch_end' block and a thunk landing pad.
            var catchEndBlock = CatchBlock.CreateChildBlock("catch_end");
            var catchExceptionBlock = CatchBlock.CreateChildBlock("catch_exception");

            var catchEndLandingPadBlock = CatchBlock.CreateChildBlock("catch_end_landingpad");
            ItaniumCxxFinallyBlock.PopulateThunkLandingPadBlock(
                catchEndLandingPadBlock,
                catchExceptionBlock.Block,
                true);

            CatchBlock = CatchBlock.WithUnwindTarget(catchEndLandingPadBlock, catchEndBlock);

            // The catch block starts like this:
            //
            // catch:
            //     %exception_data = load { i8*, i32 }* %exception_data_alloca
            //     %exception_obj = extractvalue { i8*, i32 } %exception_data, 0
            //     %exception_ptr_opaque = call i8* @__cxa_begin_catch(i8* %exception_obj)
            //     %exception_ptr = bitcast i8* %exception_ptr_opaque to i8**
            //     %exception = load i8*, i8** %exception_ptr
            //     %exception_vtable_ptr_ptr = bitcast i8* %exception to i8**
            //     %exception_vtable_ptr = load i8*, i8** %exception_vtable_ptr_ptr
            //     %exception_typeid = <typeid> i64, i8* %exception_vtable_ptr

            var exceptionData = BuildLoad(
                CatchBlock.Builder,
                CatchBlock.FunctionBody.ExceptionDataStorage.Value,
                "exception_data");

            var exceptionObj = BuildExtractValue(
                CatchBlock.Builder,
                exceptionData,
                0,
                "exception_obj");

            var exceptionPtrOpaque = BuildCall(
                CatchBlock.Builder,
                CatchBlock.FunctionBody.Module.Declare(IntrinsicValue.CxaBeginCatch),
                new LLVMValueRef[] { exceptionObj },
                "exception_ptr_opaque");

            var exceptionPtr = BuildBitCast(
                CatchBlock.Builder,
                exceptionPtrOpaque,
                PointerType(PointerType(Int8Type(), 0), 0),
                "exception_ptr");

            var exception = BuildLoad(CatchBlock.Builder, exceptionPtr, "exception");

            var exceptionVtablePtrPtr = BuildBitCast(
                CatchBlock.Builder,
                exception,
                PointerType(PointerType(Int8Type(), 0), 0),
                "exception_vtable_ptr_ptr");

            var exceptionVtablePtr = AtAddressEmitVariable.BuildConstantLoad(
                CatchBlock.Builder,
                exceptionVtablePtrPtr,
                "exception_vtable_ptr");

            var exceptionTypeid = TypeIdBlock.BuildTypeid(
                CatchBlock.Builder,
                exceptionVtablePtr);

            // Next, we need to figure out if we have a catch block that can handle the
            // exception we've thrown. We do so by iterating over all catch blocks and
            // testing if they're a match for the exception's type.
            var fallthroughBlock = CatchBlock.CreateChildBlock("catch_test");
            for (int i = 0; i < CatchClauses.Count; i++)
            {
                var clause = CatchClauses[i];

                var catchBodyBlock = CatchBlock.CreateChildBlock("catch_body");
                var catchBodyBlockTail = catchBodyBlock;

                // First, emit the catch body. This is just a regular block that
                // clears the exception value variable when it gets started and
                // jumps to the 'catch_end' when it's done.
                //
                // catch_body:
                //     %exception_val = bitcast i8* %exception to <clause_type>
                //     store <clause_type> %exception_val, <clause_type>* %clause_exception_variable_alloca
                //     <catch clause body>
                //     br catch_end

                var ehVarAddressAndBlock = clause.LLVMHeader
                    .AtAddressExceptionVariable.Address.Emit(catchBodyBlockTail);
                catchBodyBlockTail = ehVarAddressAndBlock.BasicBlock;

                BuildStore(
                    catchBodyBlockTail.Builder,
                    BuildBitCast(
                        catchBodyBlockTail.Builder,
                        exception,
                        catchBodyBlockTail.FunctionBody.Module.Declare(clause.ExceptionType),
                        "exception_val"),
                    ehVarAddressAndBlock.Value);

                catchBodyBlockTail = clause.Body.Emit(catchBodyBlockTail).BasicBlock;

                BuildBr(catchBodyBlockTail.Builder, catchEndBlock.Block);

                // Each clause is implemented as:
                //
                // catch_clause:
                //     %clause_typeid = <typeid> i64, clause-exception-type
                //     %typeid_rem = urem i64 %exception_typeid_tmp, %clause_typeid
                //     %is_subtype = cmp eq i64 %typeid_rem, 0
                //     br i1 %is_subtype, label %catch_body, label %fallthrough
                //
                var clauseTypeid = CatchBlock.FunctionBody.Module.GetTypeId((LLVMType)clause.ExceptionType);

                var typeIdRem = BuildURem(
                    CatchBlock.Builder,
                    exceptionTypeid,
                    ConstInt(exceptionTypeid.TypeOf(), clauseTypeid, false),
                    "typeid_rem");

                var isSubtype = BuildICmp(
                    CatchBlock.Builder,
                    LLVMIntPredicate.LLVMIntEQ,
                    typeIdRem,
                    ConstInt(exceptionTypeid.TypeOf(), 0, false),
                    "is_subtype");

                BuildCondBr(CatchBlock.Builder, isSubtype, catchBodyBlock.Block, fallthroughBlock.Block);

                CatchBlock = fallthroughBlock;
                fallthroughBlock = CatchBlock.CreateChildBlock("catch_test");
            }

            // If we didn't match any catch clauses, then we'll rethrow the exception and
            // unwind to the end-catch landing pad.
            //
            // no_matching_clause:
            //     invoke void @__cxa_rethrow() to label %unreachable unwind label %catch_end_landingpad
            //
            // unreachable:
            //     unreachable
            BuildInvoke(
                CatchBlock.Builder,
                CatchBlock.FunctionBody.Module.Declare(IntrinsicValue.CxaRethrow),
                new LLVMValueRef[] { },
                fallthroughBlock.Block,
                catchEndLandingPadBlock.Block,
                "");
            BuildUnreachable(fallthroughBlock.Builder);

            // The catch end block simply calls '__cxa_end_catch' and jumps to the 'leave'
            // block. Like so:
            //
            // catch_end:
            //     call void @__cxa_end_catch()
            //     br label %leave

            BuildCall(
                catchEndBlock.Builder,
                CatchBlock.FunctionBody.Module.Declare(IntrinsicValue.CxaEndCatch),
                new LLVMValueRef[] { },
                "");
            BuildBr(catchEndBlock.Builder, LeaveBlock.Block);

            // The 'catch exception' block is entered when an exception is thrown from
            // the 'catch' block, or if the catch block is ill-equipped to handle the
            // exception. Its responsibility is to end the catch block and jump to
            // the manual unwind target.
            //
            // catch_exception:
            //     call void @__cxa_end_catch()
            //     br label %manual_unwind_target

            BuildCall(
                catchExceptionBlock.Builder,
                catchExceptionBlock.FunctionBody.Module.Declare(IntrinsicValue.CxaEndCatch),
                new LLVMValueRef[] { },
                "");
            BuildBr(catchExceptionBlock.Builder, ItaniumCxxFinallyBlock.GetManualUnwindTarget(catchBlockEntry));
        }
    }
}