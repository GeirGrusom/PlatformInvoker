using System;
using System.Reflection;
using System.Reflection.Emit;
using Platform.Invoke.Attributes;

namespace Platform.Invoke
{
    public sealed class ProbingMethodCallWrapper : DefaultMethodCallWrapper
    {
        private readonly Func<FieldBuilder> probeField;

        public ProbingMethodCallWrapper(Func<FieldBuilder> probeField) 
        {
            this.probeField = probeField;
        }

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