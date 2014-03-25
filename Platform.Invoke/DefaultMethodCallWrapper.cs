using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    public interface IMethodCallWrapper
    {
        MethodBuilder GenerateInvocation(TypeBuilder owner, MethodInfo overrideMethod, IEnumerable<FieldBuilder> fieldBuilders);
    }

    public class DefaultMethodCallWrapper : IMethodCallWrapper
    {
        private readonly Func<MethodInfo, string> methodToFieldNameMapper;

        public DefaultMethodCallWrapper(Func<MethodInfo, string> methodToFieldNameMapper)
        {
            this.methodToFieldNameMapper = methodToFieldNameMapper;
        }


        protected virtual void OnInvokeBegin(TypeBuilder type, ILGenerator generator, MethodInfo interfaceMethod)
        {
            
        }


        protected virtual void OnInvokeEnd(TypeBuilder type, ILGenerator generator, MethodInfo interfaceMethod)
        {
            
        }

        public MethodBuilder GenerateInvocation(TypeBuilder owner, MethodInfo overrideMethod, IEnumerable<FieldBuilder> fieldBuilders)
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

            var field = fieldBuilders.First(f => f.Name == methodToFieldNameMapper(overrideMethod));
            var parameters = overrideMethod.GetParameters();
            OnInvokeBegin(owner, generator, overrideMethod);
            generator.Emit(OpCodes.Ldarg_0); //  this
            generator.Emit(OpCodes.Ldfld, field); // MethodNameProc _glMethodName. Initialized by constructor.
            foreach (var item in parameters.Where(p => !p.IsRetval).Select((p, i) => new { Type = p, Index = i }))
            {
                generator.Emit(OpCodes.Ldarg, item.Index + 1);
            }
            
            generator.EmitCall(OpCodes.Callvirt, field.FieldType.GetMethod("Invoke"), null);

            OnInvokeEnd(owner, generator, overrideMethod);

            generator.Emit(OpCodes.Ret);

            owner.DefineMethodOverride(result, overrideMethod);

            return result;
        }    
    }
}