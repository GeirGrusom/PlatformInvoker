using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

using Platform.Invoke.Attributes;

namespace Platform.Invoke
{

    internal static class LibraryMapperCache<TInterface>
        where TInterface : class
    {
        private readonly static TInterface instance;

        public static TInterface Instance { get { return instance; } }

        static LibraryMapperCache()
        {
            var type = typeof(TInterface);
            if(!type.IsInterface)
                throw new NotSupportedException("Only interfaces are supported at this point.");
            var attrib = type.GetCustomAttributes(typeof(LibraryAttribute), true).OfType<LibraryAttribute>().FirstOrDefault();
            if(attrib == null)
                throw new InvalidOperationException(string.Format("Interface '{0}' is missing LibraryAttribute.", type.Name));

            instance = LibraryInterfaceFactory.Implement<TInterface>(attrib.Name);
        }
    }

    /// <summary>
    /// Provides functionality for building library mappers.
    /// </summary>
    public class LibraryInterfaceFactory
    {

        /// <summary>
        /// Implements an interface using the specified library.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="library">Library to retrieve methods from.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with all methods implemented.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="library"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <typeparamref name="TInterface"/> is not an interface type.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, Func<string, string> lookupFunctionName = null )
            where TInterface : class
        {
            if(library == null)
                throw new ArgumentNullException("library");

            var mapper = new LibraryInterfaceMapper(new DelegateTypeBuilder(),
                new DefaultConstructorBuilder(lookupFunctionName),
                new DefaultMethodCallWrapper());
            return mapper.Implement<TInterface>(library);
        }

        /// <summary>
        /// Implements an interface using the specified library and a method probe.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="library">Library to retrieve methods from.</param>
        /// <param name="probe">Probe invoked before and after method invocations.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with probing invoked between calls.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="library"/> or <see paramref="probe"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, IMethodCallProbe<TInterface> probe, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            if(library == null)
                throw new ArgumentNullException("library");
            if(probe == null)
                throw new ArgumentNullException("probe");

            var constructorBuilder = new ProbingConstructorBuilder(lookupFunctionName);
            var methoCallWrapper = new ProbingMethodCallWrapper(() => constructorBuilder.ProbeField);
            var mapper = new LibraryInterfaceMapper(new DelegateTypeBuilder(), constructorBuilder, methoCallWrapper);
            return mapper.Implement<TInterface>(library, probe);
        }
        /// <summary>
        /// Implements an interface using the specified library and a method probe.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="library">Library to retrieve methods from.</param>
        /// <param name="onBegin">Method to call before a method is invoked.</param>
        /// <param name="onEnd">Method to call after a method is invoked.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with probing methods invoked between calls.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="library"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, Action<MethodInfo, TInterface> onBegin, Action<MethodInfo, TInterface> onEnd, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            return Implement(library, new ProcProbe<TInterface>(onBegin, onEnd), lookupFunctionName);
        }
        /// <summary>
        /// Implements an interface using the library specified in LibraryAttribute on the type.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with all methods implemented.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see typeparamref="TInterface"/> does not have a LibraryAttribute defined.</exception>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            var libattrib = typeof(TInterface).GetCustomAttributes(typeof(LibraryAttribute), true).OfType<LibraryAttribute>().FirstOrDefault();
            if(libattrib == null)
                throw new InvalidOperationException(string.Format("The type '{0}' is missing a LibraryAttribute.", typeof(TInterface).Name));
            return Implement<TInterface>(libattrib.Name, lookupFunctionName);

        }
        /// <summary>
        /// Implements an interface using the library specified in LibraryAttribute on the type.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="probe">Probe invoked before and after method invocations.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with probing invoked between calls.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see typeparamref="TInterface"/> does not have a LibraryAttribute defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="probe"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(IMethodCallProbe<TInterface> probe, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            var libattrib = typeof(TInterface).GetCustomAttributes(typeof(LibraryAttribute), true).OfType<LibraryAttribute>().FirstOrDefault();
            if (libattrib == null)
                throw new InvalidOperationException(string.Format("The type '{0}' is missing a LibraryAttribute.", typeof(TInterface).Name));
            return Implement(libattrib.Name, probe, lookupFunctionName);

        }

        /// <summary>
        /// Implements an interface using the library specified in LibraryAttribute on the type.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="onBegin">Method to call before a method is invoked. Can be null.</param>
        /// <param name="onEnd">Method to call after a method is invoked. Can be null.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with probing methods invoked between calls.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <see typeparamref="TInterface"/> does not have a LibraryAttribute defined.</exception>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(Action<MethodInfo, TInterface> onBegin, Action<MethodInfo, TInterface> onEnd, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            var libattrib = typeof(TInterface).GetCustomAttributes(typeof(LibraryAttribute), true).OfType<LibraryAttribute>().FirstOrDefault();
            if (libattrib == null)
                throw new InvalidOperationException(string.Format("The type '{0}' is missing a LibraryAttribute.", typeof(TInterface).Name));
            return Implement(libattrib.Name, new ProcProbe<TInterface>(onBegin, onEnd), lookupFunctionName);
        }

        /// <summary>
        /// Implements an interface using the specified library.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="library">Library to retrieve methods from.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with all methods implemented.</returns>
        /// <exception cref="DllNotFoundException">Thrown if <see paramref="library"/> could not be loaded.</exception>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(string library, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            var lib = LibraryLoaderFactory.Create().Load(library);
            if(lib == null)
                throw new DllNotFoundException(string.Format("Unable to locate the library '{0}'.", library));

            return Implement<TInterface>(lib, lookupFunctionName);
        }

        /// <summary>
        /// Implements an interface using the specified library and a method probe.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="library">Library to retrieve methods from.</param>
        /// <param name="probe">Probe invoked before and after method invocations.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with probing invoked between calls.</returns>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        /// <exception cref="DllNotFoundException">Thrown if <see paramref="library"/> could not be loaded.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(string library, IMethodCallProbe<TInterface> probe, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            var lib = LibraryLoaderFactory.Create().Load(library);
            if (lib == null)
                throw new DllNotFoundException(string.Format("Unable to locate the library '{0}'.", library));

            return Implement(lib, probe, lookupFunctionName);
        }
        /// <summary>
        /// Implements an interface using the specified library and a method probe.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="library">Library to retrieve methods from.</param>
        /// <param name="onBegin">Method to call before a method is invoked.</param>
        /// <param name="onEnd">Method to call after a method is invoked.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with probing methods invoked between calls.</returns>
        /// <exception cref="ArgumentException">Thrown if TInterface is not an interface type.</exception>
        /// <exception cref="DllNotFoundException">Thrown if <see paramref="library"/> could not be loaded.</exception>
        [Pure]
        public static TInterface Implement<TInterface>(string library, Action<MethodInfo, TInterface> onBegin, Action<MethodInfo, TInterface> onEnd, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            return Implement(library, new ProcProbe<TInterface>(onBegin, onEnd), lookupFunctionName);
        }
    }
}
