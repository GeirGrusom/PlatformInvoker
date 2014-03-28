using System;

namespace Platform.Invoke
{
    public interface ILibraryProcProvider
    {
        bool Free(IntPtr module);
        IntPtr GetProc(IntPtr module, string procName);
        Delegate GetDelegateFromFunctionPointer(IntPtr functionPointer, Type delegateType);
    }
}