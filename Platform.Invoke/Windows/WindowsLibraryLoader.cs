using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Platform.Invoke.Windows
{
    [ImmutableObject(true)]
    public class WindowsLibraryLoader : ILibraryLoader
    {
        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string filename);

        private readonly Func<string, IntPtr> proc; 

        public WindowsLibraryLoader(Func<string, IntPtr> loadProc)
        {
            proc = loadProc;
        }

        public WindowsLibraryLoader()
        {
            proc = LoadLibrary;
        }

        [Pure]
        public ILibrary Load(string libraryName)
        {
            if(libraryName == null)
                throw new ArgumentNullException("libraryName");

            var moduleHandle = proc(libraryName);

            if (moduleHandle == IntPtr.Zero)
                return null;

            return new WindowsLibrary(moduleHandle);
        }
    }

    public interface ILibraryProcProvider
    {
        bool Free(IntPtr module);
        IntPtr GetProc(IntPtr module, string procName);
        Delegate GetDelegateFromFunctionPointer(IntPtr functionPointer, Type delegateType);
    }

    [ImmutableObject(true)]
    public sealed class LibraryProcProvider : ILibraryProcProvider
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

    public class WindowsLibrary : ILibrary
    {
        private bool isDisposed;

        private readonly IntPtr handle;
        private readonly ILibraryProcProvider libraryProvider;

        public IntPtr Handle { get { return handle; }}

        public void Dispose()
        {
            libraryProvider.Free(Handle);
            isDisposed = true;
        }

        public WindowsLibrary(IntPtr moduleHandle)
        {
            if(moduleHandle == IntPtr.Zero)
                throw new ArgumentNullException("moduleHandle");

            libraryProvider = new LibraryProcProvider();

            handle = moduleHandle;
        }

        public WindowsLibrary(IntPtr moduleHandle, ILibraryProcProvider provider)
        {
            if (moduleHandle == IntPtr.Zero)
                throw new ArgumentNullException("moduleHandle");

            if(provider == null)
                throw new ArgumentNullException("provider");

            libraryProvider = provider;

            handle = moduleHandle;
        }

        [Pure]
        public Delegate GetProcedure(Type delegateType, string name)
        {
            if (isDisposed)
                throw new ObjectDisposedException("Library has been freed.");

            if(name == null)
                throw new ArgumentNullException("name");

            if(delegateType == null)
                throw new ArgumentNullException("delegateType");

            if (!typeof (Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("DelegateType must be a delegate...type...", "delegateType");

            var procAddress = libraryProvider.GetProc(Handle, name);

            if (procAddress == IntPtr.Zero)
                return null;

            return libraryProvider.GetDelegateFromFunctionPointer(procAddress, delegateType);
        }

        [Pure]
        public TDelegate GetProcedure<TDelegate>(string name)
            where TDelegate : class
        {
            if (isDisposed)
                throw new ObjectDisposedException("Library has been freed.");

            if (name == null)
                throw new ArgumentNullException("name");
            if (!typeof(Delegate).IsAssignableFrom(typeof(TDelegate)))
                throw new ArgumentException("TDelegate must be a delegate...type...", "TDelegate");

            var procAddress = libraryProvider.GetProc(Handle, name);

            if (procAddress == IntPtr.Zero)
                return default(TDelegate);

            return libraryProvider.GetDelegateFromFunctionPointer(procAddress, typeof(TDelegate)) as TDelegate;
        }
    }
}
