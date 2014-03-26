using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    public sealed class ProbingMethodCallWrapper : DefaultMethodCallWrapper
    {
        private readonly Func<FieldBuilder> probeField;

        public ProbingMethodCallWrapper(Func<FieldBuilder> probeField) 
        {
            this.probeField = probeField;
        }

        protected override void OnInvokeBegin(TypeBuilder type, ILGenerator generator, MethodInfo interfaceMethod)
        {
            var beginInvoke = typeof(IMethodCallProbe).GetMethod("OnBeginInvoke");
            
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, probeField());
            generator.Emit(OpCodes.Ldtoken, interfaceMethod);
            generator.EmitCall(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", BindingFlags.Public | BindingFlags.Static, null,  new [] { typeof(RuntimeMethodHandle)}, null), null);
            generator.EmitCall(OpCodes.Callvirt, beginInvoke, null);

            base.OnInvokeBegin(type, generator, interfaceMethod);
        }

        protected override void OnInvokeEnd(TypeBuilder type, ILGenerator generator, MethodInfo interfaceMethod)
        {
            var endInvoke = typeof(IMethodCallProbe).GetMethod("OnEndInvoke");

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, probeField());
            generator.Emit(OpCodes.Ldtoken, interfaceMethod);
            generator.EmitCall(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(RuntimeMethodHandle) }, null), null);
            generator.EmitCall(OpCodes.Callvirt, endInvoke, null);

            base.OnInvokeEnd(type, generator, interfaceMethod);
        }
    }
}