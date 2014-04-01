using System.Reflection;

namespace Platform.Invoke
{
    /// <summary>
    /// Defines an interface for function call probes.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    public interface IMethodCallProbe<in TInterface>
        where TInterface : class
    {
        /// <summary>
        /// Invoked before a function call.
        /// </summary>
        /// <param name="method">Method signature for the intended callee.</param>
        /// <param name="reference">Interface reference.</param>
        void OnBeginInvoke(MethodInfo method, TInterface reference);

        /// <summary>
        /// Invoked after a function call.
        /// </summary>
        /// <param name="method">Method signature for the callee.</param>
        /// <param name="reference">Interface reference.</param>
        void OnEndInvoke(MethodInfo method, TInterface reference);
    }
}