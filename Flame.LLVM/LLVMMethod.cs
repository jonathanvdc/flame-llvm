using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Build;
using Flame.LLVM.Codegen;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    /// <summary>
    /// A method builder for LLVM assemblies.
    /// </summary>
    public sealed class LLVMMethod : IMethodBuilder
    {
        public LLVMMethod(LLVMType DeclaringType, IMethodSignatureTemplate Template)
        {
            this.ParentType = DeclaringType;
            this.templateInstance = new MethodSignatureInstance(Template, this);
            this.codeGenerator = new LLVMCodeGenerator(this);
            this.abiVal = new Lazy<LLVMAbi>(FetchAbi);
        }

        public LLVMMethod(LLVMType DeclaringType, IMethodSignatureTemplate Template, LLVMAbi Abi)
        {
            this.ParentType = DeclaringType;
            this.templateInstance = new MethodSignatureInstance(Template, this);
            this.codeGenerator = new LLVMCodeGenerator(this);
            this.abiVal = Abi.AsLazyAbi();
        }

        /// <summary>
        /// Gets the type that declares this method.
        /// </summary>
        /// <returns>This method's declaring type.</returns>
        public LLVMType ParentType { get; private set; }

        private LLVMCodeGenerator codeGenerator;
        private CodeBlock body;

        private Lazy<LLVMAbi> abiVal;

        private LLVMAbi FetchAbi()
        {
            if (this.HasAttribute(PrimitiveAttributes.Instance.ImportAttribute.AttributeType))
            {
                return ParentType.Namespace.Assembly.ExternalAbi;
            }
            else
            {
                return ParentType.Namespace.Assembly.Abi;
            }
        }

        /// <summary>
        /// Gets the ABI for this method.
        /// </summary>
        public LLVMAbi Abi => abiVal.Value;

        private MethodSignatureInstance templateInstance;

        public IEnumerable<IMethod> BaseMethods => templateInstance.BaseMethods.Value;

        public bool IsConstructor => templateInstance.IsConstructor;

        public IEnumerable<IParameter> Parameters => templateInstance.Parameters.Value;

        public IType ReturnType => templateInstance.ReturnType.Value;

        public bool IsStatic => templateInstance.Template.IsStatic;

        public IType DeclaringType => ParentType;

        public IEnumerable<IGenericParameter> GenericParameters => Enumerable.Empty<IGenericParameter>();

        public AttributeMap Attributes => templateInstance.Attributes.Value;

        public UnqualifiedName Name => templateInstance.Name;

        public QualifiedName FullName => Name.Qualify(ParentType.FullName);

        public IMethod Build()
        {
            return this;
        }

        public ICodeGenerator GetBodyGenerator()
        {
            return codeGenerator;
        }

        public void Initialize()
        {
        }

        public void SetMethodBody(ICodeBlock Body)
        {
            this.body = (CodeBlock)Body;
        }

        /// <summary>
        /// Writes this method definitions to the given module.
        /// </summary>
        /// <param name="Module">The module to populate.</param>
        public void Emit(LLVMModuleBuilder Module)
        {
            if (this.body != null)
            {
                var func = Module.Declare(this);
                var bodyBuilder = new FunctionBodyBuilder(Module, func);
                var entryPointBuilder = bodyBuilder.AppendBasicBlock("entry");
                entryPointBuilder = codeGenerator.Prologue.Emit(entryPointBuilder);
                var codeGen = this.body.Emit(entryPointBuilder);
                BuildUnreachable(codeGen.BasicBlock.Builder);
            }
        }
    }
}

