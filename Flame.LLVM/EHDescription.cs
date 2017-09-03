using System;
using Flame.Compiler;
using Flame.Compiler.Expressions;
using Flame.LLVM.Codegen;

namespace Flame.LLVM
{
    /// <summary>
    /// Describes how exception handling is implemented.
    /// </summary>
    public abstract class EHDescription
    {
        /// <summary>
        /// Creates a code block that throws an exception object.
        /// </summary>
        /// <param name="CodeGenerator">The code generator.</param>
        /// <param name="Exception">The exception to throw.</param>
        /// <returns>A code block that throws the given exception object.</returns>
        public abstract CodeBlock EmitThrow(LLVMCodeGenerator CodeGenerator, CodeBlock Exception);
    }

    /// <summary>
    /// An exception handling implementation that sits on top of C++ exception
    /// handling for the Itanium ABI. Requires linking in libc++abi or similar.
    /// </summary>
    public sealed class ItaniumCxxEHDescription : EHDescription
    {
        private ItaniumCxxEHDescription() { }

        /// <summary>
        /// Gets an instance of the Itanium C++ exception handling description.
        /// </summary>
        /// <returns>An instance of the exception handling description.</returns>
        public static readonly ItaniumCxxEHDescription Instance = new ItaniumCxxEHDescription();

        /// <inheritdoc/>
        public override CodeBlock EmitThrow(LLVMCodeGenerator CodeGenerator, CodeBlock Exception)
        {
            throw new NotImplementedException();
        }
    }
}