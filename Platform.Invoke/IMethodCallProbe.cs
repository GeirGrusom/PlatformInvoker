using System.Reflection;

namespace Platform.Invoke
{
    public interface IMethodCallProbe<in TInterface>
        where TInterface : class
    {
        void OnBeginInvoke(MethodInfo method, TInterface reference);
        void OnEndInvoke(MethodInfo method, TInterface reference);
    }
}