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
        {
            this.Address = Address;
        }

        /// <summary>
        /// Gets the address referenced by this variable.
        /// </summary>
        /// <returns>The address for this variable.</returns>
        public CodeBlock Address { get; private set; }

        public ICodeBlock EmitAddressOf()
        {
            return Address;
        }

        public ICodeBlock EmitGet()
        {
            return new UnaryBlock(
                Address.CodeGenerator,
                Address,
                Address.Type.AsPointerType().ElementType,
                BuildLoad);
        }

        public ICodeBlock EmitRelease()
        {
            return new VoidBlock(Address.CodeGenerator);
        }

        public ICodeBlock EmitSet(ICodeBlock Value)
        {
            return new StoreBlock(Address.CodeGenerator, Address, (CodeBlock)Value);
        }
    }
}