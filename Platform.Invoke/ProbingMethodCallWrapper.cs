using System;
using System.Reflection;
using System.Reflection.Emit;
using Platform.Invoke.Attributes;

namespace Platform.Invoke
{
    /// <summary>
    /// Provides an implementation of a <see cref="IMethodCallWrapper"/> that supports function probing.
    /// </summary>
    public sealed class ProbingMethodCallWrapper : DefaultMethodCallWrapper
    {
        private readonly Func<FieldBuilder> probeField;

        /// <summary>
        /// Creates an instance of a method call wrapper with a getter for the specified probe field.
        /// </summary>
        /// <param name="probeField">Function returning a probe filed (as specified by <see cref="ProbingConstructorBuilder.ProbeField"/>)</param>
        public ProbingMethodCallWrapper(Func<FieldBuilder> probeField) 
        {
            this.probeField = probeField;
        }

        /// <summary>
        /// Implements code for begin invocations.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <param name="generator"></param>
        /// <param name="interfaceMethod"></param>
        protected override void OnInvokeBegin(TypeBuilder type, Type interfaceType, ILGenerator generator, MethodInfo interfaceMethod)
        {
            var skip = interfaceMethod.GetCustomAttribute<SkipProbeAttribute>();
            if (skip != null && (skip.SkipActions & ProbeActions.Begin) == ProbeActions.Begin)
                return;

            var probeType = typeof(IMethodCallProbe<>).MakeGenericType(interfaceType);
            var beginInvoke = probeType.GetMethod("OnBeginInvoke");
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, probeField());
            generator.Emit(OpCodes.Ldtoken, interfaceMethod);
            generator.EmitCall(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", BindingFlags.Public | BindingFlags.Static, null,  new [] { typeof(RuntimeMethodHandle)}, null), null);
            generator.Emit(OpCodes.Ldarg_0);
            generator.EmitCall(OpCodes.Callvirt, beginInvoke, null);

            base.OnInvokeBegin(type, interfaceType, generator, interfaceMethod);
        }

        /// <summary>
        /// Implements code for end invocations.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <param name="generator"></param>
        /// <param name="interfaceMethod"></param>
        protected override void OnInvokeEnd(TypeBuilder type, Type interfaceType, ILGenerator generator, MethodInfo interfaceMethod)
        {
            var skip = interfaceMethod.GetCustomAttribute<SkipProbeAttribute>();
            if ( skip != null && (skip.SkipActions & ProbeActions.End) == ProbeActions.End)
                return;

            var probeType = typeof(IMethodCallProbe<>).MakeGenericType(interfaceType);
            var endInvoke = probeType.GetMethod("OnEndInvoke");
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, probeField());
            generator.Emit(OpCodes.Ldtoken, interfaceMethod);
            generator.EmitCall(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(RuntimeMethodHandle) }, null), null);
            generator.Emit(OpCodes.Ldarg_0);
            generator.EmitCall(OpCodes.Callvirt, endInvoke, null);

            base.OnInvokeEnd(type, interfaceType, generator, interfaceMethod);
        }
    }
}