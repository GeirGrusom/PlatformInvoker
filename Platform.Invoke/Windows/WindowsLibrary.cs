using System;
using System.Diagnostics;

namespace Platform.Invoke.Windows
{
    [DebuggerDisplay("Dll({LibraryName})")]
    public sealed class WindowsLibrary : LibraryBase
    {
        public WindowsLibrary(IntPtr moduleHandle, string libraryName)
            : base(moduleHandle, new WindowsLibraryProcProvider(), libraryName)
        {
        }
    }
}