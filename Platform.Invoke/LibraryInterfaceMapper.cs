using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Invoke
{
    public sealed class LibraryInterfaceMapper
    {
        private readonly ModuleBuilder moduleBuilder;
        private readonly IDelegateTypeBuilder delegateBuilder;
        private readonly IMethodCallWrapper methodWrapper;
        private readonly IConstructorBuilder constructorBuilder;

        public LibraryInterfaceMapper(IDelegateTypeBuilder delegateBuilder, IConstructorBuilder ctorBuilder, IMethodCallWrapper methodWrapper)
        {
            if(delegateBuilder == null)
                throw new ArgumentNullException("delegateBuilder");

            if(ctorBuilder == null)
                throw new ArgumentNullException("ctorBuilder");

            if(methodWrapper == null)
                throw new ArgumentNullException("methodWrapper");

            this.constructorBuilder = ctorBuilder;
            this.delegateBuilder = delegateBuilder;
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicInterfaces"),
                AssemblyBuilderAccess.Run);

            moduleBuilder = assemblyBuilder.DefineDynamicModule("InterfaceMapping");
            this.methodWrapper = methodWrapper;
        }

        public TInterface Implement<TInterface>(ILibrary library, params object[] additionalConstructorArguments)
            where TInterface : class
        {
            var type = typeof (TInterface);
            if(!type.IsInterface)
                throw new ArgumentException("TInterface must be a...interface...type...");

            var definedType = moduleBuilder.DefineType(string.Format("{0}_Implementation", typeof(TInterface).Name),
                                         TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);

            var methods = type.GetMethods().Union(type.GetInterfaces().SelectMany(t => t.GetMethods())).ToArray();

            var fields = GenerateFields(methods, moduleBuilder, definedType);

            var constructor = constructorBuilder.GenerateConstructor(definedType, methods, fields);

            definedType.AddInterfaceImplementation(type);

            foreach (var method in methods)
            {
                methodWrapper.GenerateInvocation(definedType, method, fields);
            }
            
            var result = definedType.CreateType();

            try
            {
                return (TInterface) Activator.CreateInstance(result, new object[] {library}.Concat(additionalConstructorArguments).ToArray());
            }
            catch (TargetInvocationException ex)
            {
                // TargetInvocationException is ugly.
                throw ex.InnerException;
            }
        }

        private IList<FieldBuilder> GenerateFields(IEnumerable<MethodInfo> methods, ModuleBuilder delegateModule, TypeBuilder builder)
        {
            return (from method in methods
                    let delegateType = delegateBuilder.CreateDelegateType(method, delegateModule)
                    select builder.DefineField(GetFieldNameForMethodInfo(method), 
                    delegateType, 
                    FieldAttributes.Private | FieldAttributes.InitOnly)).ToList();
        }

        internal static string GetFieldNameForMethodInfo(MethodInfo method)
        {
            return string.Format("_{0}", method.Name);
        }
    }
}
