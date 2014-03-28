using System;

using NUnit.Framework;

namespace Platform.Invoke.Tests
{

    public class TestLibraryLoader : LibraryLoaderBase
    {
        public TestLibraryLoader(Func<string, IntPtr> loadProc) : base(loadProc)
        {
        }

        protected override ILibrary CreateLibrary(IntPtr handle, string libraryName)
        {
            return null;
        }
    }

    
    [TestFixture]
    public class LibraryLoaderBase_Tests
    {
        [Test]
        public void LibraryLoaderBase_Ok()
        {
            // Arrange
            var loader = new TestLibraryLoader(s => new IntPtr(1));

            // Act
            var result = loader.Load("Foo");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void LibraryLoaderBase_NullString_ThrowsArgumentNullException()
        {
            // Arrange
            var loader = new TestLibraryLoader(s => new IntPtr(1));

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => loader.Load(null));

            // Assert
            Assert.AreEqual("libraryName", ex.ParamName);
        }

        [Test]
        public void LibraryLoaderBase_ReturnsNullHandle_ReturnsNull()
        {
            // Arrange
            var loader = new TestLibraryLoader(s => IntPtr.Zero);

            // Act
            var result = loader.Load("Foo");

            // Assert
            Assert.IsNull(result);
        }
    }
}
