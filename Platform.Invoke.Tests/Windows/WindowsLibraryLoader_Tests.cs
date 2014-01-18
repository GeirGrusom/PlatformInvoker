using System;
using NSubstitute;
using NUnit.Framework;
using Platform.Invoke.Windows;

namespace Platform.Invoke.Tests.Windows
{
    // These tests are integrationt tests!
    [TestFixture]
    public class WindowsLibraryLoader_Tests
    {
        [Test]
        public void WindowsLibraryLoader_Ok()
        {
            // Arrange
            var loader = new WindowsLibraryLoader(s => new IntPtr(1));

            // Act
            var result = loader.Load("Foo");

            // Assert
            Assert.IsInstanceOf<WindowsLibrary>(result);
            Assert.AreEqual(new IntPtr(1), ((WindowsLibrary)result).Handle);
        }

        [Test]
        public void WindowsLibraryLoader_NullString_ThrowsArgumentNullException()
        {
            // Arrange
            var loader = new WindowsLibraryLoader(s => new IntPtr(1));

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => loader.Load(null));

            // Assert
            Assert.AreEqual("libraryName", ex.ParamName);
        }

        [Test]
        public void WindowsLibraryLoader_ReturnsNullHandle_ReturnsNull()
        {
            // Arrange
            var loader = new WindowsLibraryLoader(s => IntPtr.Zero);

            // Act
            var result = loader.Load("Foo");

            // Assert
            Assert.IsNull(result);
        }
    }
}
