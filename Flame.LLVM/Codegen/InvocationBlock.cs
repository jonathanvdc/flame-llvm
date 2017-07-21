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
        public InvocationBlock(
            ICodeGenerator CodeGenerator,
            CodeBlock Callee,
            IEnumerable<CodeBlock> Arguments)
        {
            this.codeGen = CodeGenerator;
            this.Callee = Callee;
            this.Arguments = Arguments;
            this.retType = MethodType.GetMethod(Callee.Type).ReturnType;
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

        private ICodeGenerator codeGen;

        private IType retType;

        /// <inheritdoc/>
        public override ICodeGenerator CodeGenerator => codeGen;

        /// <inheritdoc/>
        public override IType Type => retType;

        private Tuple<LLVMValueRef[], BasicBlockBuilder> EmitArguments(
            BasicBlockBuilder BasicBlock,
            CodeBlock Target,
            IEnumerable<CodeBlock> Arguments)
        {
            int targetArgCount = Target != null ? 1 : 0;
            var argArr = Arguments.ToArray<CodeBlock>();
            var allArgs = new LLVMValueRef[targetArgCount + argArr.Length];
            if (Target != null)
            {
                var targetResult = Target.Emit(BasicBlock);
                BasicBlock = targetResult.BasicBlock;
                allArgs[0] = targetResult.Value;
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

        /// <inheritdoc/>
        public override BlockCodegen Emit(BasicBlockBuilder BasicBlock)
        {
            if (Callee is DelegateBlock)
            {
                var deleg = (DelegateBlock)Callee;
                if (deleg.Op.Equals(Operator.GetDelegate)
                    || deleg.Op.Equals(Operator.GetCurriedDelegate))
                {
                    bool hasVoidRetType = retType == PrimitiveTypes.Void;
                    var argsAndBlock = EmitArguments(BasicBlock, deleg.Target, Arguments);
                    BasicBlock = argsAndBlock.Item2;
                    var callRef = BuildCall(
                        BasicBlock.Builder,
                        BasicBlock.FunctionBody.Module.Declare(deleg.Callee),
                        argsAndBlock.Item1,
                        hasVoidRetType ? "" : "call_tmp");

                    return hasVoidRetType
                        ? new BlockCodegen(BasicBlock)
                        : new BlockCodegen(BasicBlock, callRef);
                }
                else
                {
                    throw new NotImplementedException("Virtual calls are not supported yet.");
                }
            }
            else
            {
                throw new NotImplementedException("Indirect calls are not supported yet.");
            }
        }
    }
}