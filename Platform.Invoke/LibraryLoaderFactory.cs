using System;
using System.Diagnostics.Contracts;

using Platform.Invoke.Unix;
using Platform.Invoke.Windows;

namespace Platform.Invoke
{
    /// <summary>
    /// Creates instances of library loaders for the specified platform.
    /// </summary>
    public static class LibraryLoaderFactory
    {
        /// <summary>
        /// Creates a library loader for the specified operating system platform. This class is not inteded for direct use.
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        [Pure]
        public static ILibraryLoader Create(PlatformID platform)
        {
            if(platform == PlatformID.Win32NT)
                return new WindowsLibraryLoader();

            //if(platform == PlatformID.MacOSX || platform == PlatformID.Unix)
                //return new UnixLibraryLoader();

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Creates a library loader for the current operating system platform.
        /// </summary>
        /// <returns></returns>
        [Pure]
        public static ILibraryLoader Create()
        {
            return Create(Environment.OSVersion.Platform);
        }
    }
}