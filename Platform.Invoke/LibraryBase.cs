using System;
using System.Diagnostics.Contracts;

namespace Platform.Invoke
{
    /// <summary>
    /// Implements base common functionality for operating system libraries.
    /// </summary>
    public abstract class LibraryBase : ILibrary
    {
        private bool isDisposed;

        private readonly IntPtr handle;
        private readonly ILibraryProcProvider libraryProvider;
        private readonly string libraryName;

        /// <summary>
        /// Gets the name of the library that was used to load it.
        /// </summary>
        public string Name { get { return this.libraryName; } }

        /// <summary>
        /// Gets the operating system provided handle to this library.
        /// </summary>
        public IntPtr Handle { get { return this.handle; }}

        /// <summary>
        /// Unloads the library.
        /// </summary>
        public void Dispose()
        {
            this.libraryProvider.Free(Handle);
            this.isDisposed = true;
        }

        /// <summary>
        /// Creates a new instance of the library base.
        /// </summary>
        /// <param name="moduleHandle">Operating system provided module handle.</param>
        /// <param name="provider">Provider used to implement loader functionality.</param>
        /// <param name="libraryName">Name of the loaded library.</param>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="moduleHandle"/> or <see paramref="provider"/> is null.</exception>
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

        /// <summary>
        /// Gets the procedure associated with the specified name as a delegate of the specified type.
        /// </summary>
        /// <param name="delegateType">Type of delegate to return. This cannot be a generic type.</param>
        /// <param name="name">Name of the function to look up.</param>
        /// <returns>Returns a delegate for the function if found. Returns null if the function couldn't be located.</returns>
        /// <exception cref="ArgumentException">Thrown if <see paramref="delegateType"/> is a generic type or not a delegate type.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="delegateType"/> or <see paramref="name"/> is null.</exception>
        [Pure]
        public Delegate GetProcedure(Type delegateType, string name)
        {

            if (this.isDisposed)
                throw new ObjectDisposedException("Library has been freed.");

            if(name == null)
                throw new ArgumentNullException("name");

            if(delegateType == null)
                throw new ArgumentNullException("delegateType");

            if (delegateType.IsGenericType)
                throw new ArgumentException("The provided delegate type cannot be a generic type.", "delegateType");


            if (!typeof (Delegate).IsAssignableFrom(delegateType))
                throw new ArgumentException("DelegateType must be a delegate...type...", "delegateType");

            var procAddress = this.libraryProvider.GetProc(this.Handle, name);

            if (procAddress == IntPtr.Zero)
                return null;

            return this.libraryProvider.GetDelegateFromFunctionPointer(procAddress, delegateType);
        }

        /// <summary>
        /// Gets the procedure associated with the specified name as a delegate of the specified type.
        /// </summary>
        /// <typeparam name="TDelegate">Type of delegate to return. This cannot be a generic type.</typeparam>
        /// <param name="name">Name of the function to look up.</param>
        /// <returns>Returns a delegate for the function if found. Returns null if the function couldn't be located.</returns>
        /// <exception cref="ArgumentException">Thrown if <see paramref="TDelegate"/> is a generic type or not a delegate type.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="name"/> is null.</exception>
        [Pure]
        public TDelegate GetProcedure<TDelegate>(string name)
            where TDelegate : class
        {
            if (this.isDisposed)
                throw new ObjectDisposedException("Library has been freed.");

            if (name == null)
                throw new ArgumentNullException("name");

            if (typeof(TDelegate).IsGenericType)
                throw new ArgumentException("The provided delegate type cannot be a generic type.", "TDelegate");

            if (!typeof(Delegate).IsAssignableFrom(typeof(TDelegate)))
                throw new ArgumentException("TDelegate must be a delegate...type...", "TDelegate");

            var procAddress = this.libraryProvider.GetProc(this.Handle, name);

            if (procAddress == IntPtr.Zero)
                return default(TDelegate);

            return this.libraryProvider.GetDelegateFromFunctionPointer(procAddress, typeof(TDelegate)) as TDelegate;
        }
    }
}