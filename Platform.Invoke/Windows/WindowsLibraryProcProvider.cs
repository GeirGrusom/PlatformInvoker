using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Platform.Invoke.Windows
{
    [ImmutableObject(true)]
    public sealed class WindowsLibraryProcProvider : ILibraryProcProvider
    {
        [DllImport("kernel32")]
        private static extern bool FreeLibrary([In]IntPtr module);

        [DllImport("kernel32")]
        private static extern IntPtr GetProcAddress([In]IntPtr module, [In]string procName);

        public bool Free(IntPtr module)
        {
            return FreeLibrary(module);
        }

        [Pure]
        public IntPtr GetProc(IntPtr module, string procName)
        {
            IntPtr result = GetProcAddress(module, procName);
            if (result == IntPtr.Zero)
            {
                result = GetProcAddress(module, procName + "A");
                if (result == IntPtr.Zero)
                    result = GetProcAddress(module, procName + "W");
            }

            return result;
        }

        [Pure]
        public Delegate GetDelegateFromFunctionPointer(IntPtr functionPointer, Type delegateType)
        {
            return Marshal.GetDelegateForFunctionPointer(functionPointer, delegateType);
        }
    }
}