using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.Invoke.Windows;

namespace Platform.Invoke
{
    public interface ILibrary : IDisposable
    {
        [Pure]
        Delegate GetProcedure(Type delegateType, string name);

        [Pure]
        TDelegate GetProcedure<TDelegate>(string name)
            where TDelegate : class; // Can't constraint to Delegate for some reason...
    }

    public interface ILibraryLoader
    {
        [Pure]
        ILibrary Load(string libraryName);
    }

    /// <summary>
    /// This class is used to construct the appropriate library loader for the current platform.
    /// </summary>
    public class LibraryLoaderFactory
    {
        public ILibraryLoader Create(PlatformID platform)
        {
            if(platform == PlatformID.Win32NT)
                return new WindowsLibraryLoader();
            throw new PlatformNotSupportedException();
        }
    }
}
