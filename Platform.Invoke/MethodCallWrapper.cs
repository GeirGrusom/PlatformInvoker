using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    public interface IMethodCallWrapper
    {
        void GenerateInvocation(ILGenerator generator, MethodInfo method, IEnumerable<FieldBuilder> fieldBuilders, bool emitReturn = true);
    }

    public class MethodCallWrapper : IMethodCallWrapper
    {
        private readonly Func<MethodInfo, string> methodToFieldNameMapper;

        public MethodCallWrapper(Func<MethodInfo, string> methodToFieldNameMapper)
        {
            this.methodToFieldNameMapper = methodToFieldNameMapper;
        }

        public void GenerateInvocation(ILGenerator generator, MethodInfo method, IEnumerable<FieldBuilder> fieldBuilders, bool emitReturn = true)
        {
            var field = fieldBuilders.First(f => f.Name == methodToFieldNameMapper(method));
            var parameters = method.GetParameters();

            generator.Emit(OpCodes.Nop);
            generator.Emit(OpCodes.Ldarg_0); //  this
            generator.Emit(OpCodes.Ldfld, field); // MethodNameProc _glMethodName. Initialized by constructor.
            foreach (var item in parameters.Select((p, i) => new { Type = p, Index = i }))
            {
                generator.Emit(OpCodes.Ldarg, item.Index + 1);
            }
            
            generator.EmitCall(OpCodes.Callvirt, field.FieldType.GetMethod("Invoke"), null);

            if (method.ReturnType != typeof(void) && method.GetParameters().Length == 0)
            {
                LocalBuilder local = generator.DeclareLocal(method.ReturnType);
                Label jump = generator.DefineLabel();

                generator.Emit(OpCodes.Stloc, local);
                    // Without this, the code fails on the 32-bit JIT compiler.
                generator.Emit(OpCodes.Br_S, jump);
                generator.MarkLabel(jump);
                //generator.Emit(OpCodes.Ldloc, local);
            }
            if (emitReturn)
                generator.Emit(OpCodes.Ret);
        }    
    }
}