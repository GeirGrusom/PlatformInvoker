using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Platform.Invoke
{
    public static class LibraryInterfaceFactory
    {
        /// <summary>
        /// Implements an interface using the specified library.
        /// </summary>
        /// <typeparam name="TInterface">Interface to implement using the specified library.</typeparam>
        /// <param name="library">Library to retrieve methods from.</param>
        /// <param name="lookupFunctionName">Function name transformation. Leaving this field as null will use the method name verbatim.</param>
        /// <returns>Implementation of the interface with all methods implemented.</returns>
        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, Func<string, string> lookupFunctionName = null )
            where TInterface : class
        {
            var mapper = new LibraryInterfaceMapper(new DelegateTypeBuilder(),
                new DefaultConstructorBuilder(lookupFunctionName),
                new DefaultMethodCallWrapper(f => "_" + f.Name));
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
        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, IMethodCallProbe probe, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            var constructorBuilder = new ProbingConstructorBuilder(lookupFunctionName);
            var methoCallWrapper = new ProbingMethodCallWrapper(f => "_" + f.Name, () => constructorBuilder.ProbeField);
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
        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, Action<MethodInfo> onBegin, Action<MethodInfo> onEnd, Func<string, string> lookupFunctionName = null)
            where TInterface : class
        {
            var constructorBuilder = new ProbingConstructorBuilder(lookupFunctionName);
            var methoCallWrapper = new ProbingMethodCallWrapper(f => "_" + f.Name, () => constructorBuilder.ProbeField);
            var mapper = new LibraryInterfaceMapper(new DelegateTypeBuilder(), constructorBuilder, methoCallWrapper);
            return mapper.Implement<TInterface>(library, new ProcProbe(onBegin, onEnd));
        }
    }
}
