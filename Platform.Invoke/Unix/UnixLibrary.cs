using System;
using System.Diagnostics;

namespace Platform.Invoke.Unix
{
    /// <summary>
    /// Implements library function loading using libdl.
    /// </summary>
    [DebuggerDisplay("so({LibraryName})")]
    public sealed class UnixLibrary : LibraryBase
    {
        /// <summary>
        /// Creates a new instance of a Unix library loader (using libdl).
        /// </summary>
        /// <param name="moduleHandle">Handle to the loaded module returned by dlopen(3).</param>
        /// <param name="libraryName">Name of the loaded library.</param>
        public UnixLibrary(IntPtr moduleHandle, string libraryName) 
            : base(moduleHandle, new UnixLibraryProcProvider(), libraryName)
        {
        }
    }
}