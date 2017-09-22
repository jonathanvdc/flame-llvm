using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Flame.Attributes;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Build;
using Flame.Compiler.Expressions;
using Flame.Compiler.Statements;
using Flame.Compiler.Variables;
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
        /// Gets this symbol type member's declaring type as an LLVM type.
        /// </summary>
        /// <returns>The declaring type.</returns>
        public LLVMType ParentType { get; private set; }

        /// <summary>
        /// Tests if this member is defined externally and only imported.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this member is defined externally;
        /// <c>false</c> if it is defined internally.
        /// </returns>
        public bool IsImport => IsImportedMember(this);

        private Lazy<LLVMAbi> abiVal;

        private LLVMAbi PickLLVMAbi()
        {
            return PickLLVMAbi(this, ParentType.Namespace.Assembly);
        }

        private static bool IsImportedMember(ITypeMember Member)
        {
            return Member.HasAttribute(PrimitiveAttributes.Instance.ImportAttribute.AttributeType);
        }

        private static LLVMAbi PickLLVMAbi(ITypeMember Member, LLVMAssembly DeclaringAssembly)
        {
            var abiName = LLVMAttributes.GetAbiName(Member);
            if (abiName != null)
            {
                switch (abiName.ToLowerInvariant())
                {
                    case "c":
                        return DeclaringAssembly.ExternalAbi;
                    case "c++":
                    case "c#":
                        return DeclaringAssembly.Abi;
                    default:
                        throw new InvalidDataException(
                            LLVMAttributes.AbiAttributeName + " specified unknown ABI '" + abiName + "'");
                }
            }
            else if (Member.IsStatic && IsImportedMember(Member))
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
                var linkageAttr = this.GetAttribute(LLVMLinkageAttribute.LinkageAttributeType);
                if (linkageAttr != null)
                {
                    return ((LLVMLinkageAttribute)linkageAttr).Linkage;
                }

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
                        return ParentType.Namespace.Assembly.IsWholeProgram
                            ? LLVMLinkage.LLVMInternalLinkage
                            : LLVMLinkage.LLVMExternalLinkage;
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
            this.allInterfaceImpls = new Lazy<HashSet<LLVMMethod>>(LookupAllInterfaceImpls);
        }

        public LLVMMethod(LLVMType DeclaringType, IMethodSignatureTemplate Template, LLVMAbi Abi)
            : base(DeclaringType, Abi)
        {
            this.templateInstance = new MethodSignatureInstance(Template, this);
            this.allInterfaceImpls = new Lazy<HashSet<LLVMMethod>>(LookupAllInterfaceImpls);
        }

        private LLVMCodeGenerator codeGenerator;
        private CodeBlock body;
        private Lazy<HashSet<LLVMMethod>> allInterfaceImpls;

        private MethodSignatureInstance templateInstance;

        public IEnumerable<IMethod> BaseMethods => templateInstance.BaseMethods.Value;

        public bool IsConstructor => templateInstance.IsConstructor;

        public IEnumerable<IParameter> Parameters => templateInstance.Parameters.Value;

        public IType ReturnType => templateInstance.ReturnType.Value;

        public override bool IsStatic => templateInstance.Template.IsStatic;

        public IEnumerable<IGenericParameter> GenericParameters => Enumerable.Empty<IGenericParameter>();

        public override AttributeMap Attributes => templateInstance.Attributes.Value;

        public override UnqualifiedName Name => templateInstance.Name;

        /// <summary>
        /// Gets this method's parent method: a method defined in a base class
        /// which is overriden by this method.
        /// </summary>
        /// <returns>The parent method.</returns>
        public LLVMMethod ParentMethod { get; private set; }

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
            this.codeGenerator = new LLVMCodeGenerator(this);
            ParentMethod = GetParentMethod(this);
            if (ParentMethod == this
                && this.GetIsVirtual()
                && !DeclaringType.GetIsInterface())
            {
                ParentType.RelativeVTable.CreateRelativeSlot(this);
            }
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
            if (this.GetRecursiveGenericParameters().Any<IType>())
            {
                throw new NotSupportedException("LLVM methods do not support generic parameters");
            }

            if (!DeclaringType.GetIsInterface()
                && !this.GetIsAbstract())
            {
                var func = Module.Declare(this);
                func.SetLinkage(Linkage);

                var methodBody = this.body;

                if (methodBody == null
                    && this.HasAttribute(
                        PrimitiveAttributes.Instance.RuntimeImplementedAttribute.AttributeType))
                {
                    // Auto-implement runtime-implemented methods here.
                    methodBody = (CodeBlock)AutoImplement().Emit(codeGenerator);
                }

                if (methodBody != null)
                {
                    // Generate the method body.
                    var bodyBuilder = new FunctionBodyBuilder(Module, func);
                    var entryPointBuilder = bodyBuilder.AppendBasicBlock("entry");
                    entryPointBuilder = codeGenerator.Prologue.Emit(entryPointBuilder);
                    var codeGen = methodBody.Emit(entryPointBuilder);
                    BuildUnreachable(codeGen.BasicBlock.Builder);
                }
            }

            foreach (var iface in allInterfaceImpls.Value)
            {
                Module.GetInterfaceStub(iface).Implement(ParentType, this);
            }
        }

        /// <summary>
        /// Auto-implements this method.
        /// </summary>
        private IStatement AutoImplement()
        {
            var delegateAttribute = ParentType.GetAttribute(
                MethodType.DelegateAttributeType) as IntrinsicAttribute;

            string methodName = PreMangledName.Unmangle(Name).ToString();

            var parameters = this.GetParameters();

            if (delegateAttribute != null
                && delegateAttribute.Arguments[0].GetValue<string>() == methodName)
            {
                // Implement this method using a simple invocation expression.
                var args = new IExpression[parameters.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = new ArgumentVariable(parameters[i], i).CreateGetExpression();
                }

                return new ReturnStatement(
                    new InvocationExpression(
                        new ReinterpretCastExpression(
                            new ThisVariable(DeclaringType).CreateGetExpression(),
                            MethodType.Create(this)),
                        args));
            }
            else if (methodName == "LoadDelegateHasContextInternal"
                && IsStatic
                && ReturnType == PrimitiveTypes.Boolean
                && parameters.Length == 1)
            {
                return new ReturnStatement(
                    LLVMCodeGenerator.ToExpression(
                        new UnaryBlock(
                            codeGenerator,
                            (CodeBlock)new ArgumentVariable(parameters[0], 0)
                                .CreateGetExpression()
                                .Emit(codeGenerator),
                            PrimitiveTypes.Boolean,
                            DelegateBlock.BuildLoadHasContext)));
            }
            else if (methodName == "LoadDelegateFunctionPointerInternal"
                && IsStatic
                && ReturnType.GetIsPointer()
                && parameters.Length == 1)
            {
                return new ReturnStatement(
                    LLVMCodeGenerator.ToExpression(
                        new UnaryBlock(
                            codeGenerator,
                            (CodeBlock)new ArgumentVariable(parameters[0], 0)
                                .CreateGetExpression()
                                .Emit(codeGenerator),
                            ReturnType,
                            DelegateBlock.BuildLoadFunctionPointer)));
            }
            else
            {
                throw new NotSupportedException(
                    "Runtime doesn't know how to implement method '" +
                    this.FullName.ToString() + "'.");
            }
        }

        private HashSet<LLVMMethod> LookupAllInterfaceImpls()
        {
            var results = new HashSet<LLVMMethod>();

            foreach (var baseMethod in BaseMethods)
            {
                if (baseMethod is LLVMMethod)
                {
                    if (baseMethod.DeclaringType.GetIsInterface())
                    {
                        results.Add((LLVMMethod)baseMethod);
                    }
                    else
                    {
                        results.UnionWith(((LLVMMethod)baseMethod).allInterfaceImpls.Value);
                    }
                }
            }

            return results;
        }

        private static LLVMMethod GetParentMethod(LLVMMethod Method)
        {
            var result = Method;
            foreach (var baseMethod in Method.BaseMethods)
            {
                if (baseMethod is LLVMMethod && !baseMethod.DeclaringType.GetIsInterface())
                {
                    result = (LLVMMethod)baseMethod;
                    break;
                }
            }
            return result;
        }
    }
}

