using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Platform.Invoke.Windows
{
    /// <summary>
    /// Provides an implementation for loading Windows libraries.
    /// </summary>
    [ImmutableObject(true)]
    public sealed class WindowsLibraryLoader : LibraryLoaderBase
    {
        /// <summary>
        /// Loads a library into the current running process.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>Module handle if loading was successful, otherwise null.</returns>
        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary([In]string filename);

        /// <summary>
        /// Creates an instance of a Windows library loader.
        /// </summary>
        public WindowsLibraryLoader()
            : base(LoadLibrary)
        {
        }

        /// <summary>
        /// Creates an implementation of a Windows library using the specified module handle and library name.
        /// </summary>
        /// <param name="handle">Operating system provided module handle.</param>
        /// <param name="libraryName">Name of the loaded library.</param>
        /// <returns>Implementation of <see cref="WindowsLibrary"/> for the specified arguments.</returns>
        protected override ILibrary CreateLibrary(IntPtr handle, string libraryName)
        {
            return new WindowsLibrary(handle, libraryName);
        }
    }
}
