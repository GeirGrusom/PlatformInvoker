using System;

namespace Platform.Invoke
{
    /// <summary>
    /// Provides an interface for library internal functionality.
    /// </summary>
    public interface ILibraryProcProvider
    {
        /// <summary>
        /// Frees the specified library.
        /// </summary>
        /// <param name="module">Operating system provided module handle.</param>
        /// <returns>True if the library was successfully freed, otherwise false.</returns>
        bool Free(IntPtr module);

        /// <summary>
        /// Gets a function pointer for the specified function in the specified module.
        /// </summary>
        /// <param name="module">Operating system provided module handle.</param>
        /// <param name="procName">Library function name</param>
        /// <returns>Function pointer or null if the function could not be located in the specfified module.</returns>
        IntPtr GetProc(IntPtr module, string procName);

        /// <summary>
        /// Creates a delegate for the specified function pointer.
        /// </summary>
        /// <param name="functionPointer"></param>
        /// <param name="delegateType"></param>
        /// <returns></returns>
        Delegate GetDelegateFromFunctionPointer(IntPtr functionPointer, Type delegateType);
    }
}