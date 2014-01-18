using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.Invoke.Windows;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    public class LibraryFactory_Tests
    {
        [TestCase(PlatformID.Win32S)]
        [TestCase(PlatformID.Win32Windows)]
        [TestCase(PlatformID.WinCE)]
        [TestCase(PlatformID.MacOSX)]
        [TestCase(PlatformID.Unix)]
        public void Platform_Unsupported(PlatformID platform)
        {
            // Arrange
            var factory = new LibraryLoaderFactory();

            // Act
            // Assert
            Assert.Catch<PlatformNotSupportedException>(() => factory.Create(platform));
        }

        [Test]
        public void WindowsPlatform_ReturnsWindowsImplementation()
        {
            // Arrange
            var factory = new LibraryLoaderFactory();

            // Act
            var result = factory.Create(PlatformID.Win32NT);

            // Assert
            Assert.IsInstanceOf<WindowsLibraryLoader>(result);
        }
    }
}
