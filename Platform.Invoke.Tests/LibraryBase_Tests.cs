using System;

using NSubstitute;

using NUnit.Framework;

using Platform.Invoke.Windows;

namespace Platform.Invoke.Tests
{
    public class TestLibrary : LibraryBase
    {
        public TestLibrary(IntPtr moduleHandle, ILibraryProcProvider provider, string libraryName)
            : base(moduleHandle, provider, libraryName)
        {
        }
    }


    [TestFixture]
    public class LibraryBase_Tests
    {
        [Test]
        public void Constructor_NullHandle_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new WindowsLibrary(IntPtr.Zero, null));

            Assert.AreEqual("moduleHandle", ex.ParamName);
        }

        [Test]
        public void Constructor_NullModuleHandle_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new TestLibrary(IntPtr.Zero, new WindowsLibraryProcProvider(), null));

            Assert.AreEqual("moduleHandle", ex.ParamName);
        }
        [Test]
        public void Constructor_NullProvider_ThrowsArgumentNullException()
        {
            // Arrange
            // Act
            // Assert
            var ex = Assert.Throws<ArgumentNullException>(() => new TestLibrary(new IntPtr(1), null, null));

            Assert.AreEqual("provider", ex.ParamName);
        }

        [Test]
        public void Constructor_Module_And_Provider_Ok()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();

            // Act
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Assert
            Assert.IsNotNull(lib);
        }

        [Test]
        public void GetProcuedure_NonGeneric_NameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => lib.GetProcedure(typeof (Action), null));

            // Assert
            Assert.AreEqual("name", ex.ParamName);
        }

        [Test]
        public void GetProcuedure_NonGeneric_DelegateTypeIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => lib.GetProcedure(null, "Foo"));

            // Assert
            Assert.AreEqual("delegateType", ex.ParamName);
        }

        [Test]
        public void GetProcuedure_NonGeneric_DelegateTypeIsNotADelegate_ThrowsArgumentNullException()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => lib.GetProcedure(typeof(string), "Foo"));

            // Assert
            Assert.AreEqual("delegateType", ex.ParamName);
            Assert.That(ex.Message.StartsWith("DelegateType must be a delegate...type..."));
        }

        [Test]
        public void GetProcuedure_NonGeneric_ReturnedProcAddressIsNull_ReturnsNull()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            mock.GetProc(Arg.Any<IntPtr>(), Arg.Any<string>()).Returns(IntPtr.Zero);
            mock.GetDelegateFromFunctionPointer(Arg.Any<IntPtr>(), Arg.Any<Type>())
                .Returns(new Action(GetProcuedure_NonGeneric_DelegateTypeIsNotADelegate_ThrowsArgumentNullException));
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var result = lib.GetProcedure(typeof (Action), "Foo");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetProcuedure_NonGeneric_ReturnedProcAddressIsOk_ReturnsDelegate()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            mock.GetProc(Arg.Any<IntPtr>(), Arg.Any<string>()).Returns(new IntPtr(1));
            mock.GetDelegateFromFunctionPointer(Arg.Any<IntPtr>(), Arg.Any<Type>())
                .Returns(new Action(GetProcuedure_NonGeneric_DelegateTypeIsNotADelegate_ThrowsArgumentNullException));
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var result = lib.GetProcedure(typeof(Action), "Foo");

            // Assert
            Assert.IsNotNull(result);
        }

        [Test]
        public void GetProcuedure_Generic_NameIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var ex = Assert.Throws<ArgumentNullException>(() => lib.GetProcedure<Action>(null));

            // Assert
            Assert.AreEqual("name", ex.ParamName);
        }


        [Test]
        public void GetProcuedure_Generic_DelegateTypeIsNotADelegate_ThrowsArgumentNullException()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var ex = Assert.Throws<ArgumentException>(() => lib.GetProcedure<string>("Foo"));

            // Assert
            Assert.AreEqual("TDelegate", ex.ParamName);
            Assert.That(ex.Message.StartsWith("TDelegate must be a delegate...type..."));
        }

        [Test]
        public void GetProcuedure_Generic_ReturnedProcAddressIsNull_ReturnsNull()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            mock.GetProc(Arg.Any<IntPtr>(), Arg.Any<string>()).Returns(IntPtr.Zero);
            mock.GetDelegateFromFunctionPointer(Arg.Any<IntPtr>(), Arg.Any<Type>())
                .Returns(new Action(GetProcuedure_NonGeneric_DelegateTypeIsNotADelegate_ThrowsArgumentNullException));
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var result = lib.GetProcedure<Action>("Foo");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetProcuedure_Generic_ReturnedProcAddressIsOk_ReturnsDelegate()
        {
            // Arrange
            var mock = Substitute.For<ILibraryProcProvider>();
            mock.GetProc(Arg.Any<IntPtr>(), Arg.Any<string>()).Returns(new IntPtr(1));
            mock.GetDelegateFromFunctionPointer(Arg.Any<IntPtr>(), Arg.Any<Type>())
                .Returns(new Action(GetProcuedure_NonGeneric_DelegateTypeIsNotADelegate_ThrowsArgumentNullException));
            var lib = new TestLibrary(new IntPtr(1), mock, null);

            // Act
            var result = lib.GetProcedure<Action>("Foo");

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
