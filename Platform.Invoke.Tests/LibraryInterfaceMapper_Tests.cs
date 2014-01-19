using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NSubstitute;
using NUnit.Framework;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    public class LibraryInterfaceMapper_Tests
    {
        public interface IFoo
        {
            string DoFoo();
        }

        private static string HelloWorld()
        {
            return "Hello World!";
        }

        private class MockMethodWrapper : IMethodCallWrapper
        {
            public void GenerateInvocation(ILGenerator generator, MethodInfo method, IEnumerable<FieldBuilder> fieldBuilders, bool emitReturn = true)
            {
                generator.Emit(OpCodes.Ret, "Hello World!");
            }
        }

        public class MockLibrary : ILibrary
        {
            public bool Received { get; private set; }

            public void Dispose()
            {
                
            }

            private static string Hello()
            {
                return "Hello World";
            }

            public Delegate GetProcedure(Type delegateType, string name)
            {
                Received = true;
                return new Func<string>(Hello);
            }

            public TDelegate GetProcedure<TDelegate>(string name) where TDelegate : class
            {
                Received = true;
                return new Func<string>(Hello) as TDelegate;
            }
        }

        [Test]
        public void ImplementInterface_Ok()
        {
            // Arrange
            var mockDelegateBuilder = Substitute.For<IDelegateTypeBuilder>();
            mockDelegateBuilder.CreateDelegateType(Arg.Any<MethodInfo>(), Arg.Any<ModuleBuilder>()).Returns(typeof(Func<string>));
            var mockLibrary = new MockLibrary();
            var lib = new LibraryInterfaceMapper(mockDelegateBuilder, new MockMethodWrapper());

            // Act
            var result = lib.Implement<IFoo>(mockLibrary);

            // Assert
            Assert.IsTrue(mockLibrary.Received);
        }

    }
}
