using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Compiler;
using Flame.Compiler.Visitors;
using Flame.Front;
using Flame.Front.Target;
using Flame.Front.Passes;
using Flame.Optimization;
using Flame.LLVM.Passes;

namespace Flame.LLVM
{
    public class LLVMBuildTargetParser : IBuildTargetParser
    {
        public const string LLVMIdentifier = "llvm";

        public IEnumerable<string> PlatformIdentifiers
        {
            get { return new string[] { LLVMIdentifier }; }
        }

        public bool MatchesPlatformIdentifier(string Identifier)
        {
            return PlatformIdentifiers.Contains<string>(Identifier, StringComparer.OrdinalIgnoreCase);
        }

        public string GetRuntimeIdentifier(string Identifier, ICompilerLog Log)
        {
            return LLVMIdentifier;
        }

        public BuildTarget CreateBuildTarget(string PlatformIdentifier, AssemblyCreationInfo Info, IDependencyBuilder DependencyBuilder)
        {
            var multiBinder = new MultiBinder(DependencyBuilder.Binder.Environment);
            multiBinder.AddBinder(DependencyBuilder.Binder);

            var targetAsm = new LLVMAssembly(
                new SimpleName(Info.Name),
                Info.Version,
                DependencyBuilder.Environment,
                new LLVMAbi(
                    ItaniumMangler.Instance,
                    new ExternalGCDescription(multiBinder, DependencyBuilder.Log)),
                AttributeMap.Empty);

            // -fintegrated-runtime will look in the compiled assembly for runtime types.
            // This flag facilitates building the runtime library.
            if (DependencyBuilder.Log.Options.GetFlag("integrated-runtime", false))
            {
                multiBinder.AddBinder(targetAsm.CreateBinder());
            }

            var extraPasses = new PassManager();

            // Always use -flower-box-unbox-types to lower box/unbox.
            extraPasses.RegisterMethodPass(
                new AtomicPassInfo<BodyPassArgument, IStatement>(
                    BoxUnboxTypePass.Instance,
                    BoxUnboxTypePass.BoxUnboxTypePassName));

            extraPasses.RegisterPassCondition(BoxUnboxTypePass.BoxUnboxTypePassName, UseAlways);

            // Always use -flower-string-concat to lower string concatenation to calls.
            extraPasses.RegisterMethodPass(
                new AtomicPassInfo<BodyPassArgument, IStatement>(
                    StringConcatPass.Instance,
                    StringConcatPass.StringConcatPassName));

            extraPasses.RegisterPassCondition(StringConcatPass.StringConcatPassName, UseAlways);

            // Always use -flower-string-literals to lower string literals to calls.
            extraPasses.RegisterMethodPass(
                new AtomicPassInfo<BodyPassArgument, IStatement>(
                    StringLiteralPass.Instance,
                    StringLiteralPass.StringLiteralPassName));

            extraPasses.RegisterPassCondition(StringLiteralPass.StringLiteralPassName, UseAlways);

            // Always use -flower-new-struct, for correctness reasons.
            extraPasses.RegisterPassCondition(NewValueTypeLoweringPass.NewValueTypeLoweringPassName, UseAlways);

            // Always use -fexpand-generics-llvm to expand generic definitions.
            extraPasses.RegisterMemberLoweringPass(
                new AtomicPassInfo<MemberLoweringPassArgument, MemberConverter>(
                    new GenericsExpansionPass(NameExpandedType, NameExpandedMethod),
                    GenericsExpansionPass.GenericsExpansionPassName + "-llvm"));

            extraPasses.RegisterPassCondition(
                GenericsExpansionPass.GenericsExpansionPassName + "-llvm",
                UseAlways);

            // Use -finternalize-generics to keep generic definitions from creeping
            // into assemblies that are not compiled with -fwhole-program.
            extraPasses.RegisterSignaturePass(
                new AtomicPassInfo<MemberSignaturePassArgument<IMember>, MemberSignaturePassResult>(
                    GenericsInternalizingPass.Instance,
                    GenericsInternalizingPass.GenericsInternalizingPassName));

            extraPasses.RegisterPassCondition(
                GenericsInternalizingPass.GenericsInternalizingPassName,
                UseAlways);

            return new BuildTarget(targetAsm, DependencyBuilder, "ll", true, extraPasses.ToPreferences());
        }

        private bool UseAlways(OptimizationInfo Info)
        {
            return true;
        }

        private static UnqualifiedName NameExpandedType(IType Type)
        {
            return new PreMangledName(ItaniumMangler.Instance.Mangle(Type, false));
        }

        private static UnqualifiedName NameExpandedMethod(IMethod Method)
        {
            return new PreMangledName(ItaniumMangler.Instance.Mangle(Method, false));
        }
    }
}
