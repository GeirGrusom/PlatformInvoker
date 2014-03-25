using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    public interface IDelegateTypeBuilder
    {
        [Pure]
        Type CreateDelegateType(MethodInfo method, ModuleBuilder module);
    }

    /// <summary>
    /// This class is used to construct a delegate type from a method definition. Attributes are copied as well.
    /// </summary>
    [ImmutableObject(true)]
    public class DelegateTypeBuilder : IDelegateTypeBuilder
    {
        [Pure]
        public Type CreateDelegateType(MethodInfo method, ModuleBuilder module)
        {
            var name = string.Format("{0}Proc", method.Name);

            var oldType = module.GetType(name);
            if (oldType != null)
                return oldType;

            var typeBuilder = module.DefineType(
                name, TypeAttributes.Sealed | TypeAttributes.Public, typeof(MulticastDelegate));

            var constructor = typeBuilder.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) });

            constructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            var invokeMethod = typeBuilder.DefineMethod(
                "Invoke",
                MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public,
                method.ReturnType,
                method.GetParameters().Select(p => p.ParameterType).ToArray());

            invokeMethod.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            // Copy return type attributes
            if (method.ReturnType != typeof(void))
            {
                ProcessReturnParameterAttributes(method, invokeMethod);
            }

            foreach (var param in method.GetParameters())
            {
                var par = invokeMethod.DefineParameter(param.Position + 1, param.Attributes, param.Name);
                CopyParameterAttributes(par, param);
            }

            return typeBuilder.CreateType();
        }

        private static void CopyParameterAttributes(ParameterBuilder builder, ParameterInfo parameter)
        {
            foreach (var attrib in parameter.CustomAttributes)
            {
                if (attrib.NamedArguments == null || attrib.NamedArguments.Count == 0)
                    continue;

                // The marshaller will prefer to use the MarshalType over MarshalTypeRef.
                // and will automatically set MarshalType if you specify MarshalTypeRef.
                // this will make it unable to locate the type (since without assembly specification, it will look in the dynamic assembly)
                // Therefore we have to remove MarshalType if both MarshalType and MarshalTypeRef is set.
                var namedArguments = FixMarshalTypeAttributes(attrib.NamedArguments).ToArray();
                var attribBuilder = new CustomAttributeBuilder(
                    attrib.Constructor,
                    attrib.ConstructorArguments.Select(a => a.Value).ToArray(),
                    attrib.NamedArguments.Where(a => !a.IsField)
                        .Select(s => s.MemberInfo)
                        .OfType<PropertyInfo>()
                        .ToArray(),
                    attrib.NamedArguments.Where(a => !a.IsField)
                        .Select(s => s.TypedValue)
                        .Select(s => s.Value)
                        .ToArray(),
                    namedArguments.Where(a => a.IsField).Select(s => s.MemberInfo).OfType<FieldInfo>().ToArray(),
                    namedArguments.Where(a => a.IsField).Select(s => s.TypedValue).Select(s => s.Value).ToArray());
                
                builder.SetCustomAttribute(attribBuilder);
            }
        }

        private static IEnumerable<CustomAttributeNamedArgument> FixMarshalTypeAttributes(
            IList<CustomAttributeNamedArgument> namedArguments)
        {
            if (namedArguments.Any(f => f.MemberName == "MarshalTypeRef") && namedArguments.Any(f => f.MemberName == "MarshalType"))
            {
                return namedArguments.Except(namedArguments.Where(f => f.MemberName == "MarshalType"),
                    new CustomAttributeNamedArgumentComparer()).ToArray();
            }
            return namedArguments;
        }

        private static void ProcessReturnParameterAttributes(MethodInfo method, MethodBuilder invokeMethod)
        {
            invokeMethod.SetReturnType(method.ReturnType);
            if (method.ReturnParameter != null)
            {
                var returnParameter = invokeMethod.DefineParameter(0, method.ReturnParameter.Attributes,
                    method.ReturnParameter.Name);
                CopyParameterAttributes(returnParameter, method.ReturnParameter);
            }
        }
    }
}
