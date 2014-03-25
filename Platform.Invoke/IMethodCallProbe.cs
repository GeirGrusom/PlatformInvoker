using System.Reflection;

namespace Platform.Invoke
{
    public interface IMethodCallProbe
    {
        void OnBeginInvoke(MethodInfo method);
        void OnEndInvoke(MethodInfo method);
    }
}