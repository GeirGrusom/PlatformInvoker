using System;
using System.Diagnostics.Contracts;

using Platform.Invoke.Windows;

namespace Platform.Invoke
{
    /// <summary>
    /// This class is used to construct the appropriate library loader for the current platform.
    /// </summary>
    public static class LibraryLoaderFactory
    {
        [Pure]
        public static ILibraryLoader Create(PlatformID platform)
        {
            if(platform == PlatformID.Win32NT)
                return new WindowsLibraryLoader();
            throw new PlatformNotSupportedException();
        }

        [Pure]
        public static ILibraryLoader Create()
        {
            return Create(Environment.OSVersion.Platform);
        }

    }
}