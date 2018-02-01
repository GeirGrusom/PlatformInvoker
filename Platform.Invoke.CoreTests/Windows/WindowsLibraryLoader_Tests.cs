
namespace Platform.Invoke.CoreTests.Windows
{
    using System;
    using NUnit.Framework;
    using Invoke.Windows;

    [TestFixture]
    public class WindowsLibraryLoader_Tests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                Assert.Inconclusive("Test is not applicable to this operating system.");
            }
        }

        [Test]
        public void Load_Kernel32_ReturnsHandle()
        {
            // Arrange
            var libLoader = new WindowsLibraryLoader();

            // Act
            var lib = libLoader.Load("kernel32");

            // Assert
            Assert.That(lib, Is.Not.Null);
        }

        [Test]
        public void Load_UndefinedModule_ReturnsZero()
        {
            // Arrange
            var libLoader = new WindowsLibraryLoader();

            // Act
            var lib = libLoader.Load("foooooooobar123");

            // Assert
            Assert.That(lib, Is.Null);
        }
    }
}
