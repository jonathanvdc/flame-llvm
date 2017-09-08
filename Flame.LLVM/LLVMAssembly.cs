using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Flame.Binding;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Build;
using Flame.Compiler.Expressions;
using Flame.Compiler.Statements;
using Flame.Compiler.Variables;
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
            LLVMAbi Abi,
            AttributeMap Attributes,
            bool IsWholeProgram)
        {
            this.Name = Name;
            this.Abi = Abi;
            this.ExternalAbi = new LLVMAbi(CMangler.Instance, Abi.GarbageCollector, Abi.ExceptionHandling);
            this.AssemblyVersion = AssemblyVersion;
            this.Attributes = Attributes;
            this.Environment = Environment;
            this.rootNamespace = new LLVMNamespace(new SimpleName(""), default(QualifiedName), this);
            this.IsWholeProgram = IsWholeProgram;
        }

        public Version AssemblyVersion { get; private set; }

        public AttributeMap Attributes { get; private set; }

        /// <summary>
        /// Gets the ABI for functions defined by this assembly.
        /// </summary>
        /// <returns>The ABI.</returns>
        public LLVMAbi Abi { get; private set; }

        /// <summary>
        /// Gets the ABI that is used for externally-defined functions.
        /// </summary>
        public LLVMAbi ExternalAbi { get; private set; }

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

        /// <summary>
        /// Gets the entry point method for this assembly.
        /// </summary>
        /// <returns>The entry point.</returns>
        public LLVMMethod EntryPoint { get; private set; }

        /// <summary>
        /// Tells if this assembly is a whole program---its definitions
        /// will not be used by other programs.
        /// </summary>
        /// <returns><c>true</c> if this assembly is a whole program; otherwise, <c>false</c>.</returns>
        public bool IsWholeProgram { get; private set; }

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
            return EntryPoint;
        }

        public void SetEntryPoint(IMethod Method)
        {
            this.EntryPoint = (LLVMMethod)Method;
        }

        public void Initialize()
        {

        }

        public void Save(IOutputProvider OutputProvider)
        {
            // Create the module.
            LLVMModuleRef module = ToModule();

            // Verify it.
            // IntPtr error;
            // VerifyModule(module, LLVMVerifierFailureAction.LLVMPrintMessageAction, out error);
            // DisposeMessage(error);

            // Write it to the output file.
            var file = OutputProvider.Create();
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
            var moduleBuilder = new LLVMModuleBuilder(this, module);
            rootNamespace.Emit(moduleBuilder);
            if (EntryPoint != null)
            {
                SynthesizeEntryPointThunk(moduleBuilder);
            }
            moduleBuilder.EmitStubs();
            return module;
        }

        /// <summary>
        /// Synthesizes an entry point "thunk" function that calls the user-defined
        /// entry point.
        /// </summary>
        /// <param name="Module">The LLVM module to synthesize the entry point in.</param>
        private void SynthesizeEntryPointThunk(LLVMModuleBuilder Module)
        {
            // Generate the following class:
            //
            // public static class __entry_point
            // {
            //     [#builtin_abi("C")]
            //     [#builtin_llvm_linkage(external)]
            //     public static int main(int argc, byte** argv)
            //     {
            //         return actual_entry_point(...);
            //         // --or--
            //         actual_entry_point(...);
            //         return 0;
            //     }
            // }

            var epTypeProto = new DescribedType(new SimpleName("__entry_point"), rootNamespace);
            epTypeProto.AddAttribute(PrimitiveAttributes.Instance.StaticTypeAttribute);
            var epType = new LLVMType(rootNamespace, new TypePrototypeTemplate(epTypeProto));
            var mainProto = new DescribedMethod(
                new SimpleName("main"), epType, PrimitiveTypes.Int32, true);
            mainProto.AddAttribute(new LLVMLinkageAttribute(LLVMLinkage.LLVMExternalLinkage));
            mainProto.AddParameter(new DescribedParameter("argc", PrimitiveTypes.Int32));
            mainProto.AddParameter(
                new DescribedParameter(
                    "argv",
                    PrimitiveTypes.UInt8
                    .MakePointerType(PointerKind.TransientPointer)
                    .MakePointerType(PointerKind.TransientPointer)));

            var mainThunk = new LLVMMethod(epType, new MethodPrototypeTemplate(mainProto), ExternalAbi);
            var epCall = CreateEntryPointCall(mainThunk);
            var mainBody = epCall.Type.GetIsInteger()
                ? (IStatement)new ReturnStatement(
                    new StaticCastExpression(epCall, PrimitiveTypes.Int32).Simplify())
                : new BlockStatement(new IStatement[]
                    {
                        new ExpressionStatement(epCall),
                        new ReturnStatement(new IntegerExpression(0))
                    });

            mainThunk.SetMethodBody(mainBody.Emit(mainThunk.GetBodyGenerator()));
            mainThunk.Emit(Module);
        }

        private static IType[] GetParameterTypes(IMethod Method)
        {
            return Method.Parameters
                .Select<IParameter, IType>(GetParameterType)
                .ToArray<IType>();
        }

        /// <summary>
        /// Creates an expression that calls the user-defined entry point.
        /// </summary>
        /// <param name="EntryPointThunk">
        /// The entry point thunk whose parameters are forwarded to the user-defined entry point.
        /// </param>
        /// <returns>A call to the user-defined entry point.</returns>
        private IExpression CreateEntryPointCall(LLVMMethod EntryPointThunk)
        {
            var paramTypes = GetParameterTypes(EntryPoint);
            var thunkParamTypes = GetParameterTypes(EntryPointThunk);

            if (paramTypes.Length == 0)
            {
                // Empty parameter list.
                return new InvocationExpression(EntryPoint, null, new IExpression[] { });
            }
            else if (paramTypes.Length == 2
                && thunkParamTypes.Length == 2
                && paramTypes[0].IsEquivalent(thunkParamTypes[0])
                && paramTypes[1].IsEquivalent(thunkParamTypes[1]))
            {
                // Forward parameters.
                var thunkParams = EntryPointThunk.GetParameters();
                return new InvocationExpression(
                    EntryPoint,
                    null,
                    new IExpression[]
                    {
                        new ArgumentVariable(thunkParams[0], 0).CreateGetExpression(),
                        new ArgumentVariable(thunkParams[1], 1).CreateGetExpression()
                    });
            }
            else
            {
                throw new NotSupportedException(
                    "Unsupported entry point signature; " +
                    "signature must be one of: " +
                    "int|void Main(), int|void Main(int, byte**)");
            }
        }

        private static IType GetParameterType(IParameter Parameter)
        {
            return Parameter.ParameterType;
        }
    }
}

