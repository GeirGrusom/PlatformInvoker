using System;
using System.Diagnostics;

namespace Platform.Invoke.Windows
{
    /// <summary>
    /// Provides a library implementation for Windows libraries.
    /// </summary>
    [DebuggerDisplay("Dll({LibraryName})")]
    public sealed class WindowsLibrary : LibraryBase
    {
        /// <summary>
        /// Creates a new instance of a Windows library using the specified module handle and library name.
        /// </summary>
        /// <param name="moduleHandle">operating system provided module handle.</param>
        /// <param name="libraryName">Name of the loaded library.</param>
        public WindowsLibrary(IntPtr moduleHandle, string libraryName)
            : base(moduleHandle, new WindowsLibraryProcProvider(), libraryName)
        {
        }
    }
}