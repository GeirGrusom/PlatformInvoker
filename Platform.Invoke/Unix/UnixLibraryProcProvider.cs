using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Platform.Invoke.Unix
{
    [ImmutableObject(true)]
    public sealed class UnixLibraryProcProvider : ILibraryProcProvider
    {
        [DllImport("ld")]
        private static extern IntPtr dlsym(IntPtr handle, [In]string symbolName);

        [DllImport("ld")]
        private static extern int dlclose(IntPtr handle);

        public bool Free(IntPtr module)
        {
            return dlclose(module) == 0;
        }

        [Pure]
        public IntPtr GetProc(IntPtr module, string procName)
        {
            IntPtr result = dlsym(module, procName);
            if (result == IntPtr.Zero)
            {
                result = dlsym(module, procName + "A");
                if (result == IntPtr.Zero)
                    result = dlsym(module, procName + "W");
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