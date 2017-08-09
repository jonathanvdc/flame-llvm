using System;
using Flame.Compiler;
using Flame.Compiler.Emit;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM.Codegen
{
    /// <summary>
    /// An LLVM emit variable that is a reference to an address.
    /// </summary>
    public sealed class AtAddressEmitVariable : IUnmanagedEmitVariable
    {
        public AtAddressEmitVariable(CodeBlock Address)
            : this(Address, false)
        { }

        public AtAddressEmitVariable(CodeBlock Address, bool IsConst)
        {
            this.Address = Address;
            this.IsConst = IsConst;
        }

        /// <summary>
        /// Gets the address referenced by this variable.
        /// </summary>
        /// <returns>The address for this variable.</returns>
        public CodeBlock Address { get; private set; }

        /// <summary>
        /// Tells if the object pointed to by this at-adress variable
        /// is constant, i.e., it will never change.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object pointed to by this variable is constant; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConst { get; private set; }

        public ICodeBlock EmitAddressOf()
        {
            return Address;
        }

        public ICodeBlock EmitGet()
        {
            Func<LLVMBuilderRef, LLVMValueRef, string, LLVMValueRef> loadPtr;
            if (IsConst)
                loadPtr = BuildConstantLoad;
            else
                loadPtr = BuildLoad;

            return new UnaryBlock(
                Address.CodeGenerator,
                Address,
                Address.Type.AsPointerType().ElementType,
                loadPtr);
        }

        public ICodeBlock EmitRelease()
        {
            return new VoidBlock(Address.CodeGenerator);
        }

        public ICodeBlock EmitSet(ICodeBlock Value)
        {
            return new StoreBlock(Address.CodeGenerator, Address, (CodeBlock)Value);
        }

        /// <summary>
        /// Creates a load instruction that loads from a constant memory location.
        /// </summary>
        public static LLVMValueRef BuildConstantLoad(
            LLVMBuilderRef Builder,
            LLVMValueRef Value,
            string Name)
        {
            var loadInstr = BuildLoad(Builder, Value, Name);
            loadInstr.SetMetadata(
                GetMDKindID("invariant.load"),
                MDNode(new LLVMValueRef[] { }));
            return loadInstr;
        }

        private static uint GetMDKindID(string Name)
        {
            return LLVMSharp.LLVM.GetMDKindID(Name, (uint)Name.Length);
        }
    }
}