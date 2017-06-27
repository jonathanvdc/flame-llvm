using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Flame.Compiler.Build;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    /// <summary>
    /// A namespace builder for LLVM assemblies.
    /// </summary>
    public sealed class LLVMNamespace : INamespaceBuilder, INamespaceBranch
    {
        public LLVMNamespace(
            UnqualifiedName Name,
            QualifiedName FullName,
            LLVMAssembly Assembly)
        {
            this.Name = Name;
            this.FullName = FullName;
            this.Assembly = Assembly;
            this.declaredTypes = new List<LLVMType>();
            this.declaredNamespaces = new List<LLVMNamespace>();
        }

        private List<LLVMType> declaredTypes;
        private List<LLVMNamespace> declaredNamespaces;

        /// <summary>
        /// Gets the LLVM assembly that declares this namespace.
        /// </summary>
        /// <returns>The declaring assembly.</returns>
        public LLVMAssembly Assembly { get; private set; }

        /// <summary>
        /// Gets the name of this namespace.
        /// </summary>
        /// <returns>The namespace's name.</returns>
        public UnqualifiedName Name { get; private set; }

        /// <summary>
        /// Gets the qualified name of this namespace.
        /// </summary>
        /// <returns>The namespace's qualified name.</returns>
        public QualifiedName FullName { get; private set; }

        /// <summary>
        /// Gets the assembly that declares this namespace.
        /// </summary>
        public IAssembly DeclaringAssembly => Assembly;

        /// <summary>
        /// Gets the attribute map for this namespace.
        /// </summary>
        public AttributeMap Attributes => AttributeMap.Empty;

        /// <summary>
        /// Gets a sequence of all types declared directly by this namespace. 
        /// </summary>
        public IEnumerable<IType> Types => declaredTypes;

        /// <summary>
        /// Gets a sequence of all namespaces declared directly by this namespace. 
        /// </summary>
        public IEnumerable<INamespaceBranch> Namespaces => declaredNamespaces;

        public INamespace Build()
        {
            return this;
        }

        public INamespaceBuilder DeclareNamespace(string Name)
        {
            var childName = new SimpleName(Name);
            var childNamespace = new LLVMNamespace(
                childName, childName.Qualify(FullName), Assembly);
            declaredNamespaces.Add(childNamespace);
            return childNamespace;
        }

        public ITypeBuilder DeclareType(ITypeSignatureTemplate Template)
        {
            var childType = new LLVMType(this, Template);
            declaredTypes.Add(childType);
            return childType;
        }

        public void Initialize()
        {
        }
    }
}

