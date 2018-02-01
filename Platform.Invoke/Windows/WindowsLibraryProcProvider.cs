using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Platform.Invoke.Windows
{
    /// <summary>
    /// Provides an internal implementation of Windows libraru functionality.
    /// </summary>
    [ImmutableObject(true)]
    public sealed class WindowsLibraryProcProvider : ILibraryProcProvider
    {
        /// <summary>
        /// Frees a library loaded with LoadLibrary.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern bool FreeLibrary([In]IntPtr module);

        /// <summary>
        /// Retrieves a function poiunter for the specified procedure name in the specified module.
        /// </summary>
        /// <param name="module">Operating system provided module handle.</param>
        /// <param name="procName">Library function name</param>
        /// <returns>Function pointer or null if the function could not be located.</returns>
        [DllImport("kernel32")]
        private static extern IntPtr GetProcAddress([In]IntPtr module, [In]string procName);

        /// <summary>
        /// Frees the specified library.
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public bool Free(IntPtr module)
        {
            return FreeLibrary(module);
        }

        /// <summary>
        /// Retrieves a functioon pointer for the specified function in the specified module.
        /// </summary>
        /// <param name="module">Operating system provided module handle.</param>
        /// <param name="procName">Library function name.</param>
        /// <returns>Function pointer or null if function could not be located.</returns>
        [Pure]
        public IntPtr GetProc(IntPtr module, string procName)
        {
            IntPtr result = GetProcAddress(module, procName);
            if (result == IntPtr.Zero)
            {
                result = GetProcAddress(module, procName + "W");
                if (result == IntPtr.Zero)
                    result = GetProcAddress(module, procName + "A");
            }

            return result;
        }

        /// <summary>
        /// Creates a delegate for the specified function pointer.
        /// </summary>
        /// <param name="functionPointer"></param>
        /// <param name="delegateType"></param>
        /// <returns></returns>
        [Pure]
        public Delegate GetDelegateFromFunctionPointer(IntPtr functionPointer, Type delegateType)
        {
            return Marshal.GetDelegateForFunctionPointer(functionPointer, delegateType);
        }
    }
}