﻿using System;
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
            this.Module = ModuleCreateWithName(Name.ToString());
        }

        public Version AssemblyVersion { get; private set; }

        public AttributeMap Attributes { get; private set; }

        /// <summary>
        /// Gets the assembly's name.
        /// </summary>
        /// <returns>The assembly's name.</returns>
        public UnqualifiedName Name { get; private set; }

        /// <summary>
        /// Gets the LLVM module that is managed by this LLVM assembly.
        /// </summary>
        /// <returns>The LLVM module.</returns>
        public LLVMModuleRef Module { get; private set; }

        /// <summary>
        /// Gets the environment used by this LLVM assembly.
        /// </summary>
        /// <returns>The environment.</returns>
        public IEnvironment Environment { get; private set; }

        private LLVMNamespace rootNamespace;

        public QualifiedName FullName => Name.Qualify();

        public IAssembly Build()
        {
            IntPtr error;
            VerifyModule(Module, LLVMVerifierFailureAction.LLVMAbortProcessAction, out error);
            DisposeMessage(error);
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
            IntPtr moduleOutput = PrintModuleToString(Module);
            var ir = Marshal.PtrToStringAnsi(moduleOutput);
            using (var writer = new StreamWriter(file.OpenOutput()))
            {
                writer.Write(ir);
            }
            DisposeMessage(moduleOutput);
        }

        public void SetEntryPoint(IMethod Method)
        {
            throw new NotImplementedException();
        }
    }
}
