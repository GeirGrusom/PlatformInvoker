using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    /// <summary>
    /// Defines an interface for method call wrappers.
    /// </summary>
    public interface IMethodCallWrapper
    {
        /// <summary>
        /// Creates a <see cref="MethodBuilder"/> and implements a default wrapper.
        /// </summary>
        /// <param name="owner">The type that will own this method.</param>
        /// <param name="interfaceType">Type of interface implemented by the <paramref name="owner"/>.</param>
        /// <param name="overrideMethod">Method to override.</param>
        /// <param name="fieldBuilders">Fields specified by the <see paramref="owner"/>.</param>
        /// <returns>MethodBuilder with an already implemented wrapper.</returns>
        MethodBuilder GenerateInvocation(TypeBuilder owner, Type interfaceType, MethodInfo overrideMethod, IEnumerable<FieldBuilder> fieldBuilders);
    }

    /// <summary>
    /// Provides a default implementation for method call wrappers.
    /// </summary>
    public class DefaultMethodCallWrapper : IMethodCallWrapper
    {
        /// <summary>
        /// Called to implement pre-invoke functionality.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <param name="generator"></param>
        /// <param name="interfaceMethod"></param>
        protected virtual void OnInvokeBegin(TypeBuilder type, Type interfaceType, ILGenerator generator, MethodInfo interfaceMethod)
        {

        }

        /// <summary>
        /// Called to implement post-invoke functionality.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <param name="generator"></param>
        /// <param name="interfaceMethod"></param>
        protected virtual void OnInvokeEnd(TypeBuilder type, Type interfaceType, ILGenerator generator, MethodInfo interfaceMethod)
        {

        }


        /// <summary>
        /// Creates a <see cref="MethodBuilder"/> and implements a default wrapper.
        /// </summary>
        /// <param name="owner">The type that will own this method.</param>
        /// <param name="interfaceType">Type of interface implemented by the <paramref name="owner"/>.</param>
        /// <param name="overrideMethod">Method to override.</param>
        /// <param name="fieldBuilders">Fields specified by the <see paramref="owner"/>.</param>
        /// <returns>MethodBuilder with an already implemented wrapper.</returns>
        public MethodBuilder GenerateInvocation(TypeBuilder owner, Type interfaceType, MethodInfo overrideMethod, IEnumerable<FieldBuilder> fieldBuilders)
        {

            Type[] MakeNullIfEmpty(Type[] input)
            {
                return input.Length == 0 ? null : input;
            }

            Type[][] MakeNullIfEmptyArray(Type[][] input)
            {
                return input.All(x => x == null) ? null : input;
            }

            var parameters = overrideMethod.GetParameters();
            var parameterTypes = parameters.Select(t => t.ParameterType).ToArray();
            var ret = overrideMethod.ReturnParameter;

            var parameterTypeRequiredCustomModifiers = MakeNullIfEmptyArray(parameters.Select(x => MakeNullIfEmpty(x.GetRequiredCustomModifiers())).ToArray());
            var parameterTypeOptionalCustomModifiers = MakeNullIfEmptyArray(parameters.Select(x => MakeNullIfEmpty(x.GetOptionalCustomModifiers())).ToArray());
            var returnTypeRequiredCustomModifiers = MakeNullIfEmpty(ret.GetRequiredCustomModifiers());
            var returnTypeOptionalCustomModifiers = MakeNullIfEmpty(ret.GetOptionalCustomModifiers());

            var result = owner.DefineMethod
                    (
                        name: overrideMethod.Name,
                        attributes: MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final |
                        MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                        callingConvention: overrideMethod.CallingConvention,
                        returnType: overrideMethod.ReturnType,
                        returnTypeRequiredCustomModifiers: returnTypeRequiredCustomModifiers,
                        returnTypeOptionalCustomModifiers: returnTypeOptionalCustomModifiers,
                        parameterTypes: parameterTypes,
                        parameterTypeRequiredCustomModifiers: parameterTypeRequiredCustomModifiers,
                        parameterTypeOptionalCustomModifiers: parameterTypeOptionalCustomModifiers
                    );

#if !NET35
            result.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);
#endif

            var generator = result.GetILGenerator();
            var fieldName = LibraryInterfaceMapper.GetFieldNameForMethodInfo(overrideMethod);
            var field = fieldBuilders.First(f => f.Name == fieldName);

            OnInvokeBegin(owner, interfaceType, generator, overrideMethod);
            generator.Emit(OpCodes.Ldarg_0); //  this
            generator.Emit(OpCodes.Ldfld, field); // MethodNameProc _glMethodName. Initialized by constructor.
            foreach (var item in parameters.Where(p => !p.IsRetval))
            {
                generator.Emit(OpCodes.Ldarg, item.Position + 1); // 0 is `this` so we skip it
            }

            generator.EmitCall(OpCodes.Callvirt, field.FieldType.GetMethod("Invoke"), null);

            OnInvokeEnd(owner, interfaceType, generator, overrideMethod);

            generator.Emit(OpCodes.Ret);

            owner.DefineMethodOverride(result, overrideMethod);

            return result;
        }
    }
}