using System;
using System.IO;
using System.Runtime.InteropServices;
using Flame.Binding;
using Flame.Compiler.Build;
using LLVMSharp;
using static LLVMSharp.LLVM;

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
            IEnvironment Environment,
            AttributeMap Attributes)
        {
            this.Name = Name;
            this.AssemblyVersion = AssemblyVersion;
            this.Attributes = Attributes;
            this.Environment = Environment;
            this.rootNamespace = new LLVMNamespace(new SimpleName(""), default(QualifiedName), this);
        }

        public Version AssemblyVersion { get; private set; }

        public AttributeMap Attributes { get; private set; }

        /// <summary>
        /// Gets the assembly's name.
        /// </summary>
        /// <returns>The assembly's name.</returns>
        public UnqualifiedName Name { get; private set; }

        /// <summary>
        /// Gets the environment used by this LLVM assembly.
        /// </summary>
        /// <returns>The environment.</returns>
        public IEnvironment Environment { get; private set; }

        private LLVMNamespace rootNamespace;

        public QualifiedName FullName => Name.Qualify();

        public IAssembly Build()
        {
            return this;
        }

        public IBinder CreateBinder()
        {
            return new NamespaceTreeBinder(Environment, rootNamespace);
        }

        public INamespaceBuilder DeclareNamespace(string Name)
        {
            return rootNamespace.DeclareNamespace(Name);
        }

        public IMethod GetEntryPoint()
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {

        }

        public void Save(IOutputProvider OutputProvider)
        {
            var file = OutputProvider.Create();
            var module = ToModule();
            IntPtr error;
            VerifyModule(module, LLVMVerifierFailureAction.LLVMAbortProcessAction, out error);
            DisposeMessage(error);

            IntPtr moduleOutput = PrintModuleToString(module);
            var ir = Marshal.PtrToStringAnsi(moduleOutput);
            using (var writer = new StreamWriter(file.OpenOutput()))
            {
                writer.Write(ir);
            }
            DisposeMessage(moduleOutput);
            DisposeModule(module);
        }

        public LLVMModuleRef ToModule()
        {
            var module = ModuleCreateWithName(Name.ToString());
            return module;
        }

        public void SetEntryPoint(IMethod Method)
        {
            throw new NotImplementedException();
        }
    }
}

