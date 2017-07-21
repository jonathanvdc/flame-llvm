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
    /// A base class for type members that create symbols.
    /// </summary>
    public abstract class LLVMSymbolTypeMember : ITypeMember
    {
        public LLVMSymbolTypeMember(LLVMType DeclaringType)
        {
            this.ParentType = DeclaringType;
            this.abiVal = new Lazy<LLVMAbi>(PickLLVMAbi);
        }

        public LLVMSymbolTypeMember(LLVMType DeclaringType, LLVMAbi Abi)
        {
            this.ParentType = DeclaringType;
            this.abiVal = Abi.AsLazyAbi();
        }

        /// <summary>
        /// /// Gets this symbol type member's declaring type as an LLVM type.
        /// </summary>
        /// <returns>The declaring type.</returns>
        public LLVMType ParentType { get; private set; }

        private Lazy<LLVMAbi> abiVal;

        private LLVMAbi PickLLVMAbi()
        {
            return PickLLVMAbi(this, ParentType.Namespace.Assembly);
        }

        private static LLVMAbi PickLLVMAbi(ITypeMember Member, LLVMAssembly DeclaringAssembly)
        {
            if (Member.IsStatic
                && Member.HasAttribute(PrimitiveAttributes.Instance.ImportAttribute.AttributeType))
            {
                return DeclaringAssembly.ExternalAbi;
            }
            else
            {
                return DeclaringAssembly.Abi;
            }
        }

        /// <summary>
        /// Gets the ABI for the given member.
        /// </summary>
        /// <param name="Member">The member for which an ABI is to be found.</param>
        /// <param name="DeclaringAssembly">The assembly that is examined for candidate ABIs.</param>
        /// <returns>The right ABI for the given member.</returns>
        public static LLVMAbi GetLLVMAbi(ITypeMember Member, LLVMAssembly DeclaringAssembly)
        {
            if (Member is LLVMMethod)
                return ((LLVMMethod)Member).Abi;
            else if (Member is LLVMField)
                return ((LLVMField)Member).Abi;
            else
                return PickLLVMAbi(Member, DeclaringAssembly);
        }

        /// <summary>
        /// Gets the ABI for this method.
        /// </summary>
        public LLVMAbi Abi => abiVal.Value;

        public abstract bool IsStatic { get; }
        public IType DeclaringType => ParentType;
        public abstract AttributeMap Attributes { get; }
        public abstract UnqualifiedName Name { get; }
        public QualifiedName FullName => Name.Qualify(DeclaringType.FullName);

        /// <summary>
        /// Gets the linkage for this symbol.
        /// </summary>
        /// <returns>The linkage.</returns>
        public LLVMLinkage Linkage
        {
            get
            {
                if (this.HasAttribute(
                    PrimitiveAttributes.Instance.ImportAttribute.AttributeType))
                {
                    return LLVMLinkage.LLVMExternalLinkage;
                }

                var access = this.GetAccess();
                switch (access)
                {
                    case AccessModifier.Private:
                    case AccessModifier.Assembly:
                    case AccessModifier.ProtectedAndAssembly:
                        return LLVMLinkage.LLVMInternalLinkage;
                    default:
                        return LLVMLinkage.LLVMExternalLinkage;
                }
            }
        }
    }

    /// <summary>
    /// A method builder for LLVM assemblies.
    /// </summary>
    public class LLVMMethod : LLVMSymbolTypeMember, IMethodBuilder
    {
        public LLVMMethod(LLVMType DeclaringType, IMethodSignatureTemplate Template)
            : base(DeclaringType)
        {
            this.templateInstance = new MethodSignatureInstance(Template, this);
            this.codeGenerator = new LLVMCodeGenerator(this);
        }

        public LLVMMethod(LLVMType DeclaringType, IMethodSignatureTemplate Template, LLVMAbi Abi)
            : base(DeclaringType, Abi)
        {
            this.templateInstance = new MethodSignatureInstance(Template, this);
            this.codeGenerator = new LLVMCodeGenerator(this);
        }

        private LLVMCodeGenerator codeGenerator;
        private CodeBlock body;

        private MethodSignatureInstance templateInstance;

        public IEnumerable<IMethod> BaseMethods => templateInstance.BaseMethods.Value;

        public bool IsConstructor => templateInstance.IsConstructor;

        public IEnumerable<IParameter> Parameters => templateInstance.Parameters.Value;

        public IType ReturnType => templateInstance.ReturnType.Value;

        public override bool IsStatic => templateInstance.Template.IsStatic;

        public IEnumerable<IGenericParameter> GenericParameters => Enumerable.Empty<IGenericParameter>();

        public override AttributeMap Attributes => templateInstance.Attributes.Value;

        public override UnqualifiedName Name => templateInstance.Name;

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
            var func = Module.Declare(this);
            func.SetLinkage(Linkage);
            if (this.body != null)
            {
                var bodyBuilder = new FunctionBodyBuilder(Module, func);
                var entryPointBuilder = bodyBuilder.AppendBasicBlock("entry");
                entryPointBuilder = codeGenerator.Prologue.Emit(entryPointBuilder);
                var codeGen = this.body.Emit(entryPointBuilder);
                BuildUnreachable(codeGen.BasicBlock.Builder);
            }
        }
    }
}

