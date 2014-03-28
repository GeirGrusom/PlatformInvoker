using System;
using System.ComponentModel;
using System.Reflection;

namespace Platform.Invoke
{
    [ImmutableObject(true)]
    public class ProcProbe<TInterface> : IMethodCallProbe<TInterface>
        where TInterface : class
    {
        private readonly Action<MethodInfo, TInterface> begin;

        private readonly Action<MethodInfo, TInterface> end;

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