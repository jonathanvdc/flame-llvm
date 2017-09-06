using System;
using System.Collections.Generic;
using Flame.Compiler;
using Flame.Compiler.Emit;
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

        /// <summary>
        /// Creates a try-catch-finally block.
        /// </summary>
        /// <param name="CodeGenerator">The code generator.</param>
        /// <param name="TryBody">The try block's body.</param>
        /// <param name="FinallyBody">The finally block's body.</param>
        /// <param name="CatchClauses">The catch clauses.</param>
        /// <returns>A try-catch-finally block.</returns>
        public abstract CodeBlock EmitTryCatchFinally(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock TryBody,
            CodeBlock FinallyBody,
            IReadOnlyList<CatchClause> CatchClauses);
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
            // Generate something more or less like this:
            //
            //     %1 = call i8* @__cxa_allocate_exception(i64 <sizeof(value-to-throw)>)
            //     %2 = bitcast i8* %1 to <typeof(value-to-throw)>*
            //     store i8* <value-to-throw>, <typeof(value-to-throw)>* %2
            //     call void @__cxa_throw(i8* %1, i8* bitcast (i8** @_ZTIPv to i8*), i8* null)
            //     unreachable
            //

            var exceptionType = Exception.Type;

            var exceptionStorageSpec = new DescribedVariableMember(
                "exception_storage",
                PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer));

            var exceptionStorage = CodeGenerator.DeclareLocal(
                new UniqueTag(exceptionStorageSpec.Name.ToString()),
                exceptionStorageSpec);

            var allocStmt = exceptionStorage.EmitSet(
                new InvocationBlock(
                    CodeGenerator,
                    new IntrinsicBlock(CodeGenerator, IntrinsicValue.CxaAllocateException),
                    new CodeBlock[] 
                    {
                        (CodeBlock)CodeGenerator.EmitTypeBinary(
                            CodeGenerator.EmitSizeOf(exceptionType),
                            MethodType
                                .GetMethod(IntrinsicValue.CxaAllocateException.Type)
                                .GetParameters()[0].ParameterType,
                            Operator.StaticCast)
                    },
                    false));

            var storeStmt = CodeGenerator.EmitStoreAtAddress(
                CodeGenerator.EmitTypeBinary(
                    exceptionStorage.EmitGet(),
                    exceptionType.MakePointerType(PointerKind.TransientPointer),
                    Operator.ReinterpretCast),
                Exception);

            var throwStmt = new InvocationBlock(
                CodeGenerator,
                new IntrinsicBlock(CodeGenerator, IntrinsicValue.CxaThrow),
                new CodeBlock[]
                {
                    (CodeBlock)exceptionStorage.EmitGet(),
                    (CodeBlock)CodeGenerator.EmitTypeBinary(
                        new IntrinsicBlock(CodeGenerator, IntrinsicValue.CxaVoidPointerRtti),
                        PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer),
                        Operator.ReinterpretCast),
                    (CodeBlock)CodeGenerator.EmitTypeBinary(
                        CodeGenerator.EmitNull(),
                        PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer),
                        Operator.ReinterpretCast)
                },
                true);

            return (CodeBlock)CodeGenerator.EmitSequence(
                allocStmt,
                CodeGenerator.EmitSequence(
                    storeStmt,
                    CodeGenerator.EmitSequence(
                        throwStmt,
                        new UnreachableBlock(CodeGenerator, PrimitiveTypes.Void))));
        }

        /// <inheritdoc/>
        public override CodeBlock EmitTryCatchFinally(
            LLVMCodeGenerator CodeGenerator,
            CodeBlock TryBody,
            CodeBlock FinallyBody,
            IReadOnlyList<CatchClause> CatchClauses)
        {
            return new ItaniumCxxTryBlock(CodeGenerator, TryBody, FinallyBody, CatchClauses);
        }
    }
}