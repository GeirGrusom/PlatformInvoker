using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using Platform.Invoke.Unix;
using Platform.Invoke.Windows;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    public class LibraryFactory_Tests
    {
        [TestCase(PlatformID.Win32S)] // No plans for support.
        [TestCase(PlatformID.Win32Windows)] // No plans for support.
        [TestCase(PlatformID.WinCE)] // No plans for support.
        public void Platform_Unsupported(PlatformID platform)
        {
            // Arrange
            // Act
            // Assert
            Assert.Catch<PlatformNotSupportedException>(() => LibraryLoaderFactory.Create(platform));
        }

        [Test]
        public void WindowsPlatform_ReturnsWindowsImplementation()
        {
            // Arrange

            // Act
            var result = LibraryLoaderFactory.Create(PlatformID.Win32NT);

            // Assert
            Assert.IsInstanceOf<WindowsLibraryLoader>(result);
        }

        [Test]
        public void OsXPlatform_ThrowsPlatformNotSupportedException()
        {
            // Arrange

            // Act
            var result = LibraryLoaderFactory.Create(PlatformID.MacOSX);

            // Assert
            Assert.IsInstanceOf<UnixLibraryLoader>(result);

        }

        [Test]
        public void UnixPlatform_ThrowsPlatformNotSupportedException()
        {
            // Arrange

            // Act
            var result = LibraryLoaderFactory.Create(PlatformID.MacOSX);

            // Assert
            Assert.IsInstanceOf<UnixLibraryLoader>(result);
        }
    }
}
