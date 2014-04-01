using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Invoke
{
    /// <summary>
    /// Provides a base class for operating system library loaders.
    /// </summary>
    public abstract class LibraryLoaderBase : ILibraryLoader
    {
        private readonly Func<string, IntPtr> proc; 

        /// <summary>
        /// Creates a library loader base with the specified loader procedure.
        /// </summary>
        /// <param name="loadProc">Library loader procedure.</param>
        protected LibraryLoaderBase(Func<string, IntPtr> loadProc)
        {
            proc = loadProc;
        }

        /// <summary>
        /// Creates a library function loader wrapper for the specified loaded library.
        /// </summary>
        /// <param name="handle">Operating system provided module handle.</param>
        /// <param name="libraryName">Name of the loaded library.</param>
        /// <returns>Library function pointer loader wrapper implementation.</returns>
        protected abstract ILibrary CreateLibrary(IntPtr handle, string libraryName);

        /// <summary>
        /// Loads a library with the specified name.
        /// </summary>
        /// <param name="libraryName">Name fo the library to load.</param>
        /// <returns>Library function pointer loader wrapper implementation.</returns>
        [Pure]
        public ILibrary Load(string libraryName)
        {
            if(libraryName == null)
                throw new ArgumentNullException("libraryName");

            var moduleHandle = proc(libraryName);

            if (moduleHandle == IntPtr.Zero)
                return null;

            return CreateLibrary(moduleHandle, libraryName);
        }
    }
}
