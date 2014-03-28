using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Platform.Invoke.Unix
{
    [DebuggerDisplay("so({LibraryName})")]
    public sealed class UnixLibrary : LibraryBase
    {
        public UnixLibrary(IntPtr moduleHandle, string libraryName) 
            : base(moduleHandle, new UnixLibraryProcProvider(), libraryName)
        {
        }
    }
}