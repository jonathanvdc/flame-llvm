using System;
using Flame.Build;
using LLVMSharp;
using static LLVMSharp.LLVM;

namespace Flame.LLVM
{
    /// <summary>
    /// Represents an intrinsic value: a well-known value that is injected into
    /// generated modules.
    /// </summary>
    public sealed class IntrinsicValue
    {
        /// <summary>
        /// Creates an intrinsic from the given intrinsic definition.
        /// </summary>
        /// <param name="Type">The type of the intrinsic.</param>
        /// <param name="Declare">Declares the intrinsic in a module.</param>
        public IntrinsicValue(
            IType Type,
            Func<LLVMModuleBuilder, LLVMModuleRef, LLVMValueRef> Declare)
        {
            this.Type = Type;
            this.declareIntrinsic = Declare;
        }

        /// <summary>
        /// Gets the intrinsic's type.
        /// </summary>
        /// <returns>The intrinsic's type.</returns>
        public IType Type { get; private set; }

        private Func<LLVMModuleBuilder, LLVMModuleRef, LLVMValueRef> declareIntrinsic;

        /// <summary>
        /// Declares this intrinsic in the given module.
        /// </summary>
        /// <param name="ModuleBuilder">The module builder for the module to declare the intrinsic in.</param>
        /// <param name="LLVMModule">The module to declare the intrinsic in.</param>
        /// <returns>The intrinsic's declaration.</returns>
        public LLVMValueRef Declare(LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            return declareIntrinsic(ModuleBuilder, LLVMModule);
        }

        /// <summary>
        /// An intrinsic that represents the '__cxa_allocate_exception' C++ ABI function.
        /// </summary>
        /// <remarks>
        /// Signature: void* __cxa_allocate_exception(size_t thrown_size) throw()
        /// </remarks>
        public static readonly IntrinsicValue CxaAllocateException;

        private static readonly DescribedMethod CxaAllocateExceptionSignature;

        private static LLVMValueRef DeclareCxaAllocateException(
            LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            return DeclareFromSignature(CxaAllocateExceptionSignature, ModuleBuilder, LLVMModule);
        }

        /// <summary>
        /// An intrinsic that represents the '__cxa_throw' C++ ABI function.
        /// </summary>
        /// <remarks>
        /// Signature: void __cxa_throw(void* thrown_object, std::type_info* tinfo, void (*dest)(void*))
        /// </remarks>
        public static readonly IntrinsicValue CxaThrow;

        private static readonly DescribedMethod CxaThrowSignature;

        private static LLVMValueRef DeclareCxaThrow(
            LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            return DeclareFromSignature(CxaThrowSignature, ModuleBuilder, LLVMModule);
        }

        /// <summary>
        /// An intrinsic that represents the '__cxa_rethrow' C++ ABI function.
        /// </summary>
        /// <remarks>
        /// Signature: void __cxa_rethrow()
        /// </remarks>
        public static readonly IntrinsicValue CxaRethrow;

        private static readonly DescribedMethod CxaRethrowSignature;

        private static LLVMValueRef DeclareCxaRethrow(
            LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            return DeclareFromSignature(CxaRethrowSignature, ModuleBuilder, LLVMModule);
        }

        /// <summary>
        /// An intrinsic that represents the '__cxa_begin_catch' C++ ABI function.
        /// </summary>
        /// <remarks>
        /// Signature: void* __cxa_begin_catch(void* exception_obj)
        /// </remarks>
        public static readonly IntrinsicValue CxaBeginCatch;

        private static readonly DescribedMethod CxaBeginCatchSignature;

        private static LLVMValueRef DeclareCxaBeginCatch(
            LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            return DeclareFromSignature(CxaBeginCatchSignature, ModuleBuilder, LLVMModule);
        }

        /// <summary>
        /// An intrinsic that represents the '__cxa_end_catch' C++ ABI function.
        /// </summary>
        /// <remarks>
        /// Signature: void __cxa_end_catch()
        /// </remarks>
        public static readonly IntrinsicValue CxaEndCatch;

        private static readonly DescribedMethod CxaEndCatchSignature;

        private static LLVMValueRef DeclareCxaEndCatch(
            LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            return DeclareFromSignature(CxaEndCatchSignature, ModuleBuilder, LLVMModule);
        }

        /// <summary>
        /// An intrinsic that represents the '__gxx_personality_v0' C++ ABI function.
        /// </summary>
        /// <remarks>
        /// Signature: declare i32 @__gxx_personality_v0(...)
        /// </remarks>
        public static readonly IntrinsicValue GxxPersonalityV0;

        private static LLVMValueRef DeclareGxxPersonalityV0(
            LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            var funcType = FunctionType(Int32Type(), new LLVMTypeRef[] { }, true);
            var result = AddFunction(LLVMModule, "__gxx_personality_v0", funcType);
            return result;
        }

        /// <summary>
        /// An intrinsic that represents the 'void*' C++ RTTI ('_ZTIPv').
        /// </summary>
        /// <remarks>
        /// Signature: @_ZTIPv = external constant i8*
        /// </remarks>
        public static readonly IntrinsicValue CxaVoidPointerRtti;

        private static LLVMValueRef DeclareCxaVoidPointerRtti(
            LLVMModuleBuilder ModuleBuilder, LLVMModuleRef LLVMModule)
        {
            var type = PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer);
            var result = ModuleBuilder.DeclareGlobal(ModuleBuilder.Declare(type), "_ZTIPv");
            result.SetGlobalConstant(true);
            return result;
        }

        static IntrinsicValue()
        {
            // Signature: void* __cxa_allocate_exception(size_t thrown_size) throw()
            CxaAllocateExceptionSignature = new DescribedMethod(
                "__cxa_allocate_exception",
                null,
                PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer),
                true);

            CxaAllocateExceptionSignature.AddParameter(
                new DescribedParameter("thrown_size", PrimitiveTypes.UInt64));

            CxaAllocateException = new IntrinsicValue(
                MethodType.Create(CxaAllocateExceptionSignature),
                DeclareCxaAllocateException);

            // Signature: void __cxa_throw(void* thrown_object, std::type_info* tinfo, void (*dest)(void*))
            CxaThrowSignature = new DescribedMethod(
                "__cxa_throw",
                null,
                PrimitiveTypes.Void,
                true);

            CxaThrowSignature.AddParameter(
                new DescribedParameter(
                    "thrown_object",
                    PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer)));

            CxaThrowSignature.AddParameter(
                new DescribedParameter(
                    "tinfo",
                    PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer)));

            CxaThrowSignature.AddParameter(
                new DescribedParameter(
                    "dest",
                    PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer)));

            CxaThrow = new IntrinsicValue(
                MethodType.Create(CxaThrowSignature),
                DeclareCxaThrow);

            // Signature: void __cxa_rethrow()
            CxaRethrowSignature = new DescribedMethod(
                "__cxa_rethrow",
                null,
                PrimitiveTypes.Void,
                true);

            CxaRethrow = new IntrinsicValue(
                MethodType.Create(CxaRethrowSignature),
                DeclareCxaRethrow);

            // Signature: void* __cxa_begin_catch(void* exception_obj)
            CxaBeginCatchSignature = new DescribedMethod(
                "__cxa_begin_catch",
                null,
                PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer),
                true);

            CxaBeginCatchSignature.AddParameter(
                new DescribedParameter(
                    "exception_obj",
                    PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer)));

            CxaBeginCatch = new IntrinsicValue(
                MethodType.Create(CxaBeginCatchSignature),
                DeclareCxaBeginCatch);

            // Signature: void __cxa_end_catch()
            CxaEndCatchSignature = new DescribedMethod(
                "__cxa_end_catch",
                null,
                PrimitiveTypes.Void,
                true);

            CxaEndCatch = new IntrinsicValue(
                MethodType.Create(CxaEndCatchSignature),
                DeclareCxaEndCatch);

            // Signature: declare i32 @__gxx_personality_v0(...)
            GxxPersonalityV0 = new IntrinsicValue(
                PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer),
                DeclareGxxPersonalityV0);

            // Signature: @_ZTIPv = external constant i8*
            CxaVoidPointerRtti = new IntrinsicValue(
                PrimitiveTypes.Void.MakePointerType(PointerKind.TransientPointer),
                DeclareCxaVoidPointerRtti);
        }

        private static LLVMValueRef DeclareFromSignature(
            IMethod Signature,
            LLVMModuleBuilder ModuleBuilder,
            LLVMModuleRef LLVMModule)
        {
            return AddFunction(
                LLVMModule,
                Signature.Name.ToString(),
                ModuleBuilder.DeclarePrototype(Signature));
        }
    }
}