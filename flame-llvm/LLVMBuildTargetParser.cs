using System;
using System.Collections.Generic;
using System.Linq;
using Flame.Compiler;
using Flame.Compiler.Visitors;
using Flame.Front;
using Flame.Front.Target;
using Flame.Front.Passes;
using Flame.Optimization;

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
            var targetAsm = new LLVMAssembly(
                new SimpleName(Info.Name), Info.Version,
                DependencyBuilder.Environment, AttributeMap.Empty);

            var extraPasses = new PassManager();

            // Always use -flower-new-struct, for correctness reasons.
            extraPasses.RegisterPassCondition(new PassCondition(NewValueTypeLoweringPass.NewValueTypeLoweringPassName, UseAlways));

            return new BuildTarget(targetAsm, DependencyBuilder, "ll", true, extraPasses.ToPreferences());
        }

        private bool UseAlways(OptimizationInfo Info)
        {
            return true;
        }
    }
}
