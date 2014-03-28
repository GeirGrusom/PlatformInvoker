using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Platform.Invoke.Windows
{
    [ImmutableObject(true)]
    public sealed class WindowsLibraryLoader : LibraryLoaderBase
    {
        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary([In]string filename);

        public WindowsLibraryLoader()
            : base(LoadLibrary)
        {
        }

        protected override ILibrary CreateLibrary(IntPtr handle, string libraryName)
        {
            return new WindowsLibrary(handle, libraryName);
        }
    }
}
