using System;
using System.ComponentModel;
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
            if (this.begin != null)
                this.begin(method);
        }


        void IMethodCallProbe.OnEndInvoke(MethodInfo method)
        {
            if (this.end != null)
                this.end(method);
        }
    }
}