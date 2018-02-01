using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    /// <summary>
    /// Provides an interface for library interface mappers.
    /// </summary>
    public interface ILibraryInterfaceMapper
    {
        /// <summary>
        /// Implements the specified interface using the specified library.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement.</typeparam>
        /// <param name="library">Library to implement <see typeparamref="TInterface" /> with.</param>
        /// <param name="additionalConstructorArguments">Additional parameters required by the <see cref="IConstructorBuilder"/>. <see cref="DefaultConstructorBuilder" /> requires no additional parameters. <see cref="ProbingConstructorBuilder"/> requires one parameter of type <see cref="IMethodCallProbe{TInterface}"/>.</param>
        /// <returns>Implemented interface.</returns>
        TInterface Implement<TInterface>(ILibrary library, params object[] additionalConstructorArguments)
            where TInterface : class;
    }

    /// <summary>
    /// Implements the default library interface mapper using the specified <see cref="IDelegateTypeBuilder"/>, <see cref="IMethodCallWrapper"/> and <see cref="IConstructorBuilder"/>.
    /// </summary>
    public sealed class LibraryInterfaceMapper : ILibraryInterfaceMapper
    {

        private readonly IDelegateTypeBuilder delegateBuilder;
        private readonly IMethodCallWrapper methodWrapper;
        private readonly IConstructorBuilder constructorBuilder;

        [NonSerialized]
        private static readonly AssemblyBuilder assemblyBuilder;
        [NonSerialized]
        private static readonly ModuleBuilder moduleBuilder;

        static LibraryInterfaceMapper()
        {
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicInterfaces"), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("InterfaceMapping");
        }

        /// <summary>
        /// Creates an instance of a LibraryInterfaceMapper using the specified builder.
        /// </summary>
        /// <param name="delegateBuilder"></param>
        /// <param name="ctorBuilder"></param>
        /// <param name="methodWrapper"></param>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="delegateBuilder"/>, <see paramref="ctorBuilder"/> or <see paramref="methodWrapper"/> is null.</exception>
        public LibraryInterfaceMapper(IDelegateTypeBuilder delegateBuilder, IConstructorBuilder ctorBuilder, IMethodCallWrapper methodWrapper)
        {

            if (delegateBuilder == null)
                throw new ArgumentNullException("delegateBuilder");

            if (ctorBuilder == null)
                throw new ArgumentNullException("ctorBuilder");

            if (methodWrapper == null)
                throw new ArgumentNullException("methodWrapper");


            this.constructorBuilder = ctorBuilder;
            this.delegateBuilder = delegateBuilder;

            this.methodWrapper = methodWrapper;
        }


        private IEnumerable<Type> GetTypeInterfaces(Type type)
        {
            var interf = type.GetInterfaces();
            return interf.Union(interf.SelectMany(GetTypeInterfaces));
        }

        /// <summary>
        /// Implements the interface using the speicified library source.
        /// </summary>
        /// <typeparam name="TInterface">Interface or abstract class defining the library contract.</typeparam>
        /// <param name="library">Library to use for implementation.</param>
        /// <param name="additionalConstructorArguments">Additional constructor arguments may be required by special constructor builders (such as <see cref="ProbingConstructorBuilder"/>).</param>
        /// <returns>Instance of the interface implementation.</returns>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface of abstract class.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="library"/> is null.</exception>
        /// <exception cref="MissingMethodException">Thrown if the specified method could not be located by the library.</exception>
        public TInterface Implement<TInterface>(ILibrary library, params object[] additionalConstructorArguments)
            where TInterface : class
        {
            var type = typeof(TInterface);
            if (!(type.IsInterface || type.IsAbstract))
                throw new ArgumentException("TInterface must be a interface or abstract class.");

            if (library == null)
                throw new ArgumentNullException("library");

            TypeBuilder definedType;

            MethodInfo[] methods;
            if (type.IsInterface)
            {
                methods = type.GetMethods().Concat(GetTypeInterfaces(type).SelectMany(x => x.GetMethods())).ToArray();
                definedType = moduleBuilder.DefineType(string.Format("{0}_Implementation", typeof(TInterface).Name),
                    TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);
                definedType.AddInterfaceImplementation(type);
            }
            else
            {
                methods =
                    type.GetMethods()
                        .Where(m => m.IsAbstract)
                        .ToArray();

                definedType = moduleBuilder.DefineType(string.Format("{0}_Implementation", typeof(TInterface).Name),
                                         TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class, typeof(TInterface));
            }

            var fields = GenerateFields(methods, moduleBuilder, definedType);

            constructorBuilder.GenerateConstructor(definedType, type, methods, fields);

            foreach (var method in methods)
            {
                methodWrapper.GenerateInvocation(definedType, type, method, fields);
            }

            var result = definedType.CreateTypeInfo();

            try
            {
                return (TInterface)Activator.CreateInstance(result, new object[] { library }.Concat(additionalConstructorArguments).ToArray());
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
            return string.Format("_{0}_{1}", method.Name, string.Join("_", method.GetParameters().Select(p => p.ParameterType.Name)));
        }
    }
}
