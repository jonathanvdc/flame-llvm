using System.Collections.Generic;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    /// <summary>
    /// Defines an interface stub, which handles a single interface method.
    /// </summary>
    public sealed class InterfaceStub
    {
        public InterfaceStub(LLVMValueRef Function)
        {
            this.Function = Function;
            this.impls = new Dictionary<LLVMType, LLVMMethod>();
        }

        /// <summary>
        /// Gets the function that implements this stub.
        /// </summary>
        /// <returns>The function that implements this stub.</returns>
        public LLVMValueRef Function { get; private set; }

        private Dictionary<LLVMType, LLVMMethod> impls;

        /// <summary>
        /// Gets a dictionary that maps implementing types to implementations.
        /// </summary>
        /// <returns>A dictionary that maps implementing types to implementations.</returns>
        public IReadOnlyDictionary<LLVMType, LLVMMethod> Implementations { get; private set; }

        /// <summary>
        /// Registers the given method as the implementation for the given type.
        /// </summary>
        /// <param name="Type">The type that implements the method.</param>
        /// <param name="Method">The implementation method.</param>
        public void Implement(LLVMType Type, LLVMMethod Method)
        {
            impls.Add(Type, Method);
        }

        private LLVMBasicBlockRef WriteReturnBlock(string Name, LLVMValueRef Result)
        {
            var block = AppendBasicBlock(Function, Name);
            var builder = CreateBuilder();
            PositionBuilderAtEnd(builder, block);
            BuildRet(builder, Result);
            DisposeBuilder(builder);
            return block;
        }

        private LLVMBasicBlockRef WriteReturnBlock(
            LLVMType Type,
            LLVMMethod Implementation,
            LLVMModuleBuilder Module)
        {
            return WriteReturnBlock(Type.Name.ToString(), Module.DeclareVirtual(Implementation));
        }

        /// <summary>
        /// Defines this stub's body.
        /// </summary>
        /// <param name="Module">The module that defines the stub.</param>
        public void Emit(LLVMModuleBuilder Module)
        {
            var entryBlock = AppendBasicBlock(Function, "entry");

            var defaultBlock = AppendBasicBlock(Function, "unknown_type");
            var builder = CreateBuilder();
            PositionBuilderAtEnd(builder, defaultBlock);
            BuildUnreachable(builder);
            DisposeBuilder(builder);

            builder = CreateBuilder();
            PositionBuilderAtEnd(builder, entryBlock);
            var switchInstr = BuildSwitch(builder, GetParam(Function, 0), defaultBlock, (uint)impls.Count);
            foreach (var pair in impls)
            {
                switchInstr.AddCase(
                    ConstInt(Int64Type(), (ulong)Module.GetTypeIndex(pair.Key), false),
                    WriteReturnBlock(pair.Key, pair.Value, Module));
            }
            DisposeBuilder(builder);
        }
    }
}