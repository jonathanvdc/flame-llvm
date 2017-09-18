using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Build;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.Compiler.Statements;
using Flame.Compiler.Variables;
using Flame.Front;
using Flame.Front.Cli;
using Flame.Front.Options;
using Flame.Front.Target;
using LLVMSharp;

namespace Flame.LLVM
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildTargetParsers.RegisterEnvironment(
                "llvm",
                (Func<ICompilerLog, IEnvironment>)CreateLLVMEnvironment);

            BuildTargetParsers.Parser.RegisterParser(new LLVMBuildTargetParser());
            var compiler = new FlameLLVMCompiler(
                "flame-llvm",
                "the Flame IR -> LLVM compiler",
                "https://github.com/jonathanvdc/flame-llvm/releases");
            Environment.Exit(compiler.Compile(args));
        }

        private static IEnvironment CreateLLVMEnvironment(ICompilerLog Log)
        {
            return new StandaloneEnvironment("llvm");
        }
    }

    internal sealed class FlameLLVMCompiler : ConsoleCompiler
    {
        /// <inheritdoc/>
        public FlameLLVMCompiler(CompilerName Name)
            : base(Name)
        { }

        /// <inheritdoc/>
        public FlameLLVMCompiler(CompilerName Name, IOptionParser<string> OptionParser)
            : base(Name, OptionParser)
        { }

        /// <inheritdoc/>
        public FlameLLVMCompiler(string Name, string FullName, string ReleasesSite)
            : base(Name, FullName, ReleasesSite)
        { }

        /// <inheritdoc/>
        public FlameLLVMCompiler(CompilerName Name, IOptionParser<string> OptionParser, ICompilerOptions DefaultOptions)
            : base(Name, OptionParser, DefaultOptions)
        { }

        /// <inheritdoc/>
        protected override Tuple<IAssembly, IEnumerable<IAssembly>> RewriteAssemblies(
            Tuple<IAssembly, IEnumerable<IAssembly>> MainAndOtherAssemblies,
            IBinder Binder,
            ICompilerLog Log)
        {
            // In addition to emitting LLVM IR from managed code, flame-llvm must also
            // set up an environment in which managed code can run. Part of this
            // environment is the 'main' function: managed code expects an entry point
            // to look like this: `void|int Main(|string[])`, whereas a C 'main' function
            // must have the following signature: `int main(int, byte**)`.
            //
            // To bridge this divide, we'll generate a 'main' function and use that to
            // call the entry point.

            var originalAsm = MainAndOtherAssemblies.Item1;
            var originalEntryPoint = originalAsm.GetEntryPoint();
            if (originalEntryPoint == null)
            {
                // We can't rewrite the entry point of an assembly that doesn't
                // have an entry point.
                return MainAndOtherAssemblies;
            }

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

            var mainAsm = new DescribedAssembly(
                originalAsm.Name,
                originalAsm.AssemblyVersion,
                Binder.Environment);

            var epType = new DescribedType(new SimpleName("__entry_point"), mainAsm);
            epType.AddAttribute(PrimitiveAttributes.Instance.StaticTypeAttribute);
            var mainThunk = new DescribedBodyMethod(
                new SimpleName("main"), epType, PrimitiveTypes.Int32, true);
            mainThunk.AddAttribute(new LLVMLinkageAttribute(LLVMLinkage.LLVMExternalLinkage));
            mainThunk.AddAttribute(LLVMAttributes.CreateAbiAttribute("C"));
            mainThunk.AddParameter(new DescribedParameter("argc", PrimitiveTypes.Int32));
            mainThunk.AddParameter(
                new DescribedParameter(
                    "argv",
                    PrimitiveTypes.UInt8
                        .MakePointerType(PointerKind.TransientPointer)
                        .MakePointerType(PointerKind.TransientPointer)));

            if (originalEntryPoint.HasSameSignature(mainThunk))
            {
                // We don't have to rewrite the entry point if the existing entry point
                // already has the expected form.
                return MainAndOtherAssemblies;
            }

            var epCall = CreateEntryPointCall(originalEntryPoint, mainThunk, Binder);
            mainThunk.Body = epCall.Type.GetIsInteger()
                ? (IStatement)new ReturnStatement(
                    new StaticCastExpression(epCall, PrimitiveTypes.Int32).Simplify())
                : new BlockStatement(new IStatement[]
                    {
                        new ExpressionStatement(epCall),
                        new ReturnStatement(new IntegerExpression(0))
                    });

            epType.AddMethod(mainThunk);
            mainAsm.AddType(epType);
            mainAsm.EntryPoint = mainThunk;

            return new Tuple<IAssembly, IEnumerable<IAssembly>>(
                mainAsm,
                new IAssembly[] { originalAsm }.Concat<IAssembly>(MainAndOtherAssemblies.Item2));
        }

        /// <summary>
        /// Creates a call to the user-defined entry point.
        /// </summary>
        /// <param name="EntryPoint">The entry point to call.</param>
        /// <param name="Caller">The method whose parameters are forwarded to the user-defined entry point.</param>
        /// <param name="Binder">A type name binder.</param>
        /// <returns>A call to the user-defined entry point.</returns>
        private static IExpression CreateEntryPointCall(
            IMethod EntryPoint,
            IMethod Caller,
            IBinder Binder)
        {
            var initMethod = BindEnvironmentInitializeMethod(Binder, Caller);

            var paramTypes = GetParameterTypes(EntryPoint);
            var thunkParamTypes = GetParameterTypes(Caller);

            if (paramTypes.Length == 0)
            {
                // Empty parameter list.
                return AfterInitialization(
                    initMethod,
                    Caller,
                    new InvocationExpression(EntryPoint, null, new IExpression[] { }));
            }
            else if (paramTypes.Length == 2
                && thunkParamTypes.Length == 2
                && paramTypes[0].IsEquivalent(thunkParamTypes[0])
                && paramTypes[1].IsEquivalent(thunkParamTypes[1]))
            {
                // Forward parameters.
                return AfterInitialization(
                    initMethod,
                    Caller,
                    CreateForwardingCall(EntryPoint, Caller));
            }
            else if (initMethod != null
                && paramTypes.Length == 1
                && paramTypes[0].IsEquivalent(initMethod.ReturnType))
            {
                // Forward the return type of the 'Initialize' method
                // to the entry point.
                return new InvocationExpression(
                    EntryPoint,
                    null,
                    new IExpression[] { CreateForwardingCall(initMethod, Caller) });
            }
            else
            {
                throw new NotSupportedException(
                    "Unsupported entry point signature; " +
                    "signature must be one of: " +
                    "int|void Main(), int|void Main(int, byte**)");
            }
        }

        private static IExpression AfterInitialization(
            IMethod InitializationMethod,
            IMethod Caller,
            IExpression Expression)
        {
            if (InitializationMethod == null)
            {
                return Expression;
            }
            else
            {
                return new InitializedExpression(
                    new ExpressionStatement(
                        CreateForwardingCall(InitializationMethod, Caller)),
                    Expression);
            }
        }

        private static IExpression CreateForwardingCall(
            IMethod Callee,
            IMethod Caller)
        {
            var thunkParams = Caller.GetParameters();
            var args = new IExpression[thunkParams.Length];
            for (int i = 0; i < args.Length; i++)
            {
                args[i] = new ArgumentVariable(thunkParams[i], i).CreateGetExpression();
            } 
            return new InvocationExpression(Callee, null, args);
        }

        private static IMethod BindEnvironmentInitializeMethod(
            IBinder Binder, IMethod MainThunk)
        {
            var envType = Binder.BindType(new SimpleName("Environment").Qualify("System"));
            if (envType == null)
            {
                return null;
            }

            return envType.GetMethod(
                new SimpleName("Initialize"),
                true,
                PrimitiveTypes.String.MakeArrayType(1),
                GetParameterTypes(MainThunk));
        }

        private static IType[] GetParameterTypes(IMethod Method)
        {
            return Method.Parameters
                .Select<IParameter, IType>(GetParameterType)
                .ToArray<IType>();
        }

        private static IType GetParameterType(IParameter Parameter)
        {
            return Parameter.ParameterType;
        }
    }
}
