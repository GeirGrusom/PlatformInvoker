using System;
using System.Diagnostics.Contracts;

namespace Platform.Invoke
{
    public abstract class LibraryBase : ILibrary
    {
        private bool isDisposed;

        private readonly IntPtr handle;
        private readonly ILibraryProcProvider libraryProvider;
        private readonly string libraryName;

        public string Name { get { return this.libraryName; } }

        public IntPtr Handle { get { return this.handle; }}

        public void Dispose()
        {
            this.libraryProvider.Free(this.Handle);
            this.isDisposed = true;
        }

        protected LibraryBase(IntPtr moduleHandle, ILibraryProcProvider provider, string libraryName)
        {
            if (moduleHandle == IntPtr.Zero)
                throw new ArgumentNullException("moduleHandle");

            if(provider == null)
                throw new ArgumentNullException("provider");

            this.libraryProvider = provider;
            this.libraryName = libraryName;

            this.handle = moduleHandle;
        }

        [Pure]
        public Delegate GetProcedure(Type delegateType, string name)
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("Library has been freed.");

            if(name == null)
                throw new ArgumentNullException("name");

            if(delegateType == null)
                throw new ArgumentNullException("delegateType");

            if (!typeof (Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("DelegateType must be a delegate...type...", "delegateType");

            var procAddress = this.libraryProvider.GetProc(this.Handle, name);

            if (procAddress == IntPtr.Zero)
                return null;

            return this.libraryProvider.GetDelegateFromFunctionPointer(procAddress, delegateType);
        }

        [Pure]
        public TDelegate GetProcedure<TDelegate>(string name)
            where TDelegate : class
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("Library has been freed.");

            if (name == null)
                throw new ArgumentNullException("name");
            if (!typeof(Delegate).IsAssignableFrom(typeof(TDelegate)))
                throw new ArgumentException("TDelegate must be a delegate...type...", "TDelegate");

            var procAddress = this.libraryProvider.GetProc(this.Handle, name);

            if (procAddress == IntPtr.Zero)
                return default(TDelegate);

            return this.libraryProvider.GetDelegateFromFunctionPointer(procAddress, typeof(TDelegate)) as TDelegate;
        }
    }
}