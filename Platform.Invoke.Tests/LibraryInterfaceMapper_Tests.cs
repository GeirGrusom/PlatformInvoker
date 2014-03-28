using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NSubstitute;
using NUnit.Framework;

using Platform.Invoke.Attributes;

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
            public MethodBuilder GenerateInvocation(TypeBuilder owner, Type interfaceType, MethodInfo overrideMethod, IEnumerable<FieldBuilder> fieldBuilders)
            {
                var result = owner.DefineMethod
                   (
                       overrideMethod.Name,
                       MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final |
                       MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                       overrideMethod.ReturnType,
                       overrideMethod.GetParameters().OrderBy(p => p.Position).Select(t => t.ParameterType).ToArray()
                   );

                var builder = result.GetILGenerator();
                owner.DefineMethodOverride(result, overrideMethod);
                builder.Emit(OpCodes.Ldstr, "Hello World!");
                builder.Emit(OpCodes.Ret);
                return result;
            }
        }

        public abstract class AbstractFoo
        {
            public abstract string Foo();
        }


        public class MockLibrary : ILibrary
        {
            public bool Received { get; private set; }

            public string Name { get { return "Mock"; } }

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
            var lib = new LibraryInterfaceMapper(mockDelegateBuilder, new DefaultConstructorBuilder(null), new MockMethodWrapper());


            // Act
            var result = lib.Implement<IFoo>(mockLibrary);

            // Assert
            Assert.IsTrue(mockLibrary.Received);
        }

        [Test]
        public void ImplementInterface_MissingMethod_ThrowsEntryPointNotFoundException()
        {
            // Arrange
            var mockDelegateBuilder = Substitute.For<IDelegateTypeBuilder>();
            mockDelegateBuilder.CreateDelegateType(Arg.Any<MethodInfo>(), Arg.Any<ModuleBuilder>()).Returns(typeof(Func<string>));
            var mockLibrary = Substitute.For<ILibrary>();
            mockLibrary.Name.Returns("FooLib");
            mockLibrary.GetProcedure<Func<string>>(Arg.Any<string>()).Returns((Func<string>)null);

            var lib = new LibraryInterfaceMapper(mockDelegateBuilder, new DefaultConstructorBuilder(null), new MockMethodWrapper());

            // Act
            // Assert
            var ex = Assert.Throws<MissingEntryPointException>(() => lib.Implement<IFoo>(mockLibrary));

            Assert.AreEqual("DoFoo", ex.EntryPoint);
            Assert.AreEqual("FooLib", ex.LibraryName);

        }

        [Test]
        public void ImplementAbstractClass_Ok()
        {
            // Arrange
            var mockDelegateBuilder = Substitute.For<IDelegateTypeBuilder>();
            mockDelegateBuilder.CreateDelegateType(Arg.Any<MethodInfo>(), Arg.Any<ModuleBuilder>()).Returns(typeof(Func<string>));
            var mockLibrary = Substitute.For<ILibrary>();
            var lib = new LibraryInterfaceMapper(mockDelegateBuilder, new DefaultConstructorBuilder(null), new MockMethodWrapper());


            // Act
            var result = lib.Implement<AbstractFoo>(mockLibrary);

            // Assert
            mockLibrary.Received(1).GetProcedure<Func<string>>("Foo");
        }

    }
}
