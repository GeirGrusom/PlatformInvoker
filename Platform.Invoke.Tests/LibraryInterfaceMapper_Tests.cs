using System;
using System.Collections.Generic;
using System.Linq;
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
            public MethodBuilder GenerateInvocation(TypeBuilder owner, MethodInfo overrideMethod, IEnumerable<FieldBuilder> fieldBuilders)
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
            var lib = new LibraryInterfaceMapper(mockDelegateBuilder, new DefaultConstructorBuilder(), new MockMethodWrapper());


            // Act
            var result = lib.Implement<IFoo>(mockLibrary);

            // Assert
            Assert.IsTrue(mockLibrary.Received);
        }

    }
}
