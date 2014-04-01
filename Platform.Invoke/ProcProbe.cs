using System;
using System.ComponentModel;
using System.Reflection;

namespace Platform.Invoke
{
    /// <summary>
    /// Implements a probe with begin and end delegates.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    [ImmutableObject(true)]
    public class ProcProbe<TInterface> : IMethodCallProbe<TInterface>
        where TInterface : class
    {
        private readonly Action<MethodInfo, TInterface> begin;

        private readonly Action<MethodInfo, TInterface> end;

        /// <summary>
        /// Creates a new probe using the specified begin and end delegates.
        /// </summary>
        /// <param name="onBegin"></param>
        /// <param name="onEnd"></param>
        public ProcProbe(Action<MethodInfo, TInterface> onBegin, Action<MethodInfo, TInterface> onEnd)
        {
            this.begin = onBegin;
            this.end = onEnd;
        }

        void IMethodCallProbe<TInterface>.OnBeginInvoke(MethodInfo method, TInterface reference)
        {
            if (this.begin != null)
                this.begin(method, reference);
        }


        void IMethodCallProbe<TInterface>.OnEndInvoke(MethodInfo method, TInterface reference)
        {
            if (this.end != null)
                this.end(method, reference);
        }
    }
}