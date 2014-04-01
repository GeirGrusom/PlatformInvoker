using System;
using System.Runtime.InteropServices;

namespace Platform.Invoke.Unix
{
    /// <summary>
    /// Library loading support for Unix operating systems (including OS X).
    /// </summary>
    public sealed class UnixLibraryLoader : LibraryLoaderBase
    {
        /// <summary>
        /// Defines flags used by dlopen.
        /// </summary>
        [Flags]
        private enum Flags
        {
            /// <summary>
            /// Perform lazy binding. Only resolve symbols as the code that references them is executed. 
            /// If the symbol is never referenced, then it is never resolved. (Lazy binding is only 
            /// performed for function references; references to variables are always immediately bound
            /// when the library is loaded.)
            /// </summary>
            Lazy = 0x00001,
            /// <summary>
            /// If this value is specified, or the environment variable LD_BIND_NOW is set to a nonempty 
            /// string, all undefined symbols in the library are resolved before dlopen() returns.
            /// If this cannot be done, an error is returned.
            /// </summary>
            Now = 0x00002,
            /// <summary>
            /// The symbols defined by this library will be made available for symbol resolution of 
            /// subsequently loaded libraries.
            /// </summary>
            Global = 0x00100,
            /// <summary>
            /// This is the converse of <see cref="Global"/>, and the default if neither flag is specified. 
            /// Symbols defined in this library are not made available to resolve references in subsequently 
            /// loaded libraries.
            /// </summary>
            Local = 0,
            /// <summary>
            /// Do not unload the library during dlclose(). Consequently, the library's static variables are
            /// not reinitialized if the library is reloaded with dlopen() at a later time. This flag is not
            /// specified in POSIX.1-2001.
            /// </summary>
            NoDelete = 0x01000,
        }

        /// <summary>
        /// Unix library loader function.
        /// </summary>
        /// <param name="filename">Filename to load,</param>
        /// <param name="flags">Loader flags. Typically RTLD_LAZY.</param>
        /// <returns>Handle to the library.</returns>
        [DllImport("libdl")]
        private static extern IntPtr dlopen([In]string filename, Flags flags);

        /// <summary>
        /// Creates an instance of UnixLibraryLoader using default implementation with dlopen.
        /// </summary>
        public UnixLibraryLoader()
            : base(s => dlopen(s, Flags.Lazy | Flags.Local))
        {
        }

        /// <summary>
        /// Returns the created library for the specified handle and library name.
        /// </summary>
        /// <param name="handle">Operating system provided library handle.</param>
        /// <param name="libraryName">Name fo the loaded library.</param>
        /// <returns><see cref="ILibrary"/> interface for the specified library.</returns>
        protected override ILibrary CreateLibrary(IntPtr handle, string libraryName)
        {
            return new UnixLibrary(handle, libraryName);
        }
    }
}
