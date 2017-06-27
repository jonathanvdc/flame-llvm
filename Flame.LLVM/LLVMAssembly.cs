using System;
using Flame.Compiler.Build;

namespace Flame.LLVM
{
    /// <summary>
    /// An assembly builder implementation that creates an LLVM module.
    /// </summary>
    public sealed class LLVMAssembly : IAssemblyBuilder
    {
        public LLVMAssembly(
            UnqualifiedName Name,
            Version AssemblyVersion,
            AttributeMap Attributes)
        {
            this.Name = Name;
            this.AssemblyVersion = AssemblyVersion;
            this.Attributes = Attributes;
        }

        public Version AssemblyVersion { get; private set; }

        public AttributeMap Attributes { get; private set; }

        public UnqualifiedName Name { get; private set; }

        public QualifiedName FullName => Name.Qualify();

        public IAssembly Build()
        {
            throw new NotImplementedException();
        }

        public IBinder CreateBinder()
        {
            throw new NotImplementedException();
        }

        public INamespaceBuilder DeclareNamespace(string Name)
        {
            throw new NotImplementedException();
        }

        public IMethod GetEntryPoint()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Save(IOutputProvider OutputProvider)
        {
            throw new NotImplementedException();
        }

        public void SetEntryPoint(IMethod Method)
        {
            throw new NotImplementedException();
        }
    }
}

