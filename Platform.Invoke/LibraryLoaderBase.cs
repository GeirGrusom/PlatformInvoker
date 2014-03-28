using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Invoke
{
    public abstract class LibraryLoaderBase : ILibraryLoader
    {
        private readonly Func<string, IntPtr> proc; 

        protected LibraryLoaderBase(Func<string, IntPtr> loadProc)
        {
            proc = loadProc;
        }

        protected abstract ILibrary CreateLibrary(IntPtr handle, string libraryName);

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
