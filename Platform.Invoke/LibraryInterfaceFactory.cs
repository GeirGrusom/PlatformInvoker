using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Platform.Invoke
{
    [ImmutableObject(true)]
    public class ProcProbe : IMethodCallProbe
    {
        private readonly Action<MethodInfo> begin;

        private readonly Action<MethodInfo> end;

        public ProcProbe(Action<MethodInfo> onBegin, Action<MethodInfo> onEnd)
        {
            this.begin = onBegin;
            this.end = onEnd;
        }

        void IMethodCallProbe.OnBeginInvoke(MethodInfo method)
        {
            if (begin != null)
                begin(method);
        }


        void IMethodCallProbe.OnEndInvoke(MethodInfo method)
        {
            if (end != null)
                end(method);
        }
    }

    public static class LibraryInterfaceFactory
    {
        /// <summary>
        /// Implements an interface using the specified library.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="library"></param>
        /// <returns></returns>
        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library)
            where TInterface : class
        {
            var mapper = new LibraryInterfaceMapper(new DelegateTypeBuilder(),
                new DefaultConstructorBuilder(),
                new DefaultMethodCallWrapper(f => "_" + f.Name));
            return mapper.Implement<TInterface>(library);
        }

        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, IMethodCallProbe probe)
            where TInterface : class
        {
            var constructorBuilder = new ProbingConstructorBuilder();
            var methoCallWrapper = new ProbingMethodCallWrapper(f => "_" + f.Name, () => constructorBuilder.ProbeField);
            var mapper = new LibraryInterfaceMapper(new DelegateTypeBuilder(), constructorBuilder, methoCallWrapper);
            return mapper.Implement<TInterface>(library, probe);
        }

        [Pure]
        public static TInterface Implement<TInterface>(ILibrary library, Action<MethodInfo> onBegin, Action<MethodInfo> onEnd)
            where TInterface : class
        {
            var constructorBuilder = new ProbingConstructorBuilder();
            var methoCallWrapper = new ProbingMethodCallWrapper(f => "_" + f.Name, () => constructorBuilder.ProbeField);
            var mapper = new LibraryInterfaceMapper(new DelegateTypeBuilder(), constructorBuilder, methoCallWrapper);
            return mapper.Implement<TInterface>(library, new ProcProbe(onBegin, onEnd));
        }
    }
}
