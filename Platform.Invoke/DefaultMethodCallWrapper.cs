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
            var result = owner.DefineMethod
                    (
                        overrideMethod.Name,
                        MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final |
                        MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                        overrideMethod.ReturnType,
                        overrideMethod.GetParameters().OrderBy(p => p.Position).Select(t => t.ParameterType).ToArray()
                    );

            result.SetImplementationFlags(MethodImplAttributes.AggressiveInlining);

            var generator = result.GetILGenerator();
            var fieldName = LibraryInterfaceMapper.GetFieldNameForMethodInfo(overrideMethod);
            var field = fieldBuilders.First(f => f.Name == fieldName);
            var parameters = overrideMethod.GetParameters();
            OnInvokeBegin(owner, interfaceType, generator, overrideMethod);
            generator.Emit(OpCodes.Ldarg_0); //  this
            generator.Emit(OpCodes.Ldfld, field); // MethodNameProc _glMethodName. Initialized by constructor.
            foreach (var item in parameters.Where(p => !p.IsRetval).Select((p, i) => new { Type = p, Index = i }))
            {
                generator.Emit(OpCodes.Ldarg, item.Index + 1);
            }
            
            generator.EmitCall(OpCodes.Callvirt, field.FieldType.GetMethod("Invoke"), null);

            OnInvokeEnd(owner, interfaceType, generator, overrideMethod);

            generator.Emit(OpCodes.Ret);

            owner.DefineMethodOverride(result, overrideMethod);

            return result;
        }    
    }
}