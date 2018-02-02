using System;
using System.Reflection;
using System.Reflection.Emit;
using NSubstitute;
using NUnit.Framework;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    public class DefaultMethodCallWrapper_Tests
    {
        private AssemblyBuilder assembly;
        private ModuleBuilder module;

        public interface IFoo
        {
            string Foo();
        }

        public interface IFooWithOutString
        {
            void Foo(out string result);
        }

        public interface IInParameter
        {
            void Foo(in int value);
        }

        public interface IFooWithString
        {
            string Foo_WithString(string arg);
        }

        [SetUp]
        public void Setup()
        {
            assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("TestAssembly"),
                AssemblyBuilderAccess.RunAndCollect);
            module = assembly.DefineDynamicModule("TestModule");
        }

        public static string Foo()
        {
            return "Hello World";
        }


        public delegate void OutFoo(out string arg);

        public delegate void InFoo(in int arg);


        public static void OutFooProc(out string arg)
        {
            arg = "Hello World!";
        }

        [Test]
        public void GeneratorInvocation_InParameter_Ok()
        {
            // Arrange
            var wrapper = new DefaultMethodCallWrapper();
            var infoo = Substitute.For<InFoo>();
            var type = module.DefineType(Guid.NewGuid().ToString());
            type.AddInterfaceImplementation(typeof(IInParameter));
            var fooString = typeof(IInParameter).GetMethod("Foo");

            var field = type.DefineField("_Foo_Int32&", typeof(InFoo), FieldAttributes.Public);

            // Act
            var resultMethod = wrapper.GenerateInvocation(type, typeof(InFoo), fooString, new[] { field });

            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo_Int32&").SetValue(obj, infoo);
            int result = 100;
            ((IInParameter)obj).Foo(result);

            infoo.Received().Invoke(result);
        }

        [Test]
        public void GenerateInvocation_OutParameter_Ok()
        {
            // Arrange
            var wrapper = new DefaultMethodCallWrapper();
            var type = module.DefineType(Guid.NewGuid().ToString());
            type.AddInterfaceImplementation(typeof(IFooWithOutString));
            var fooString = typeof(IFooWithOutString).GetMethod("Foo");

            var field = type.DefineField("_Foo_String&", typeof(OutFoo), FieldAttributes.Public);

            // Act
            var resultMethod = wrapper.GenerateInvocation(type, typeof(OutFoo), fooString, new[] { field });

            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo_String&").SetValue(obj, new OutFoo(OutFooProc));
            string result;
            ((IFooWithOutString)obj).Foo(out result);

            Assert.AreEqual("Hello World!", result);
        }

        [Test]
        public void GenerateInvocation_NoParameters_Ok()
        {
            // Arrange
            var wrapper = new DefaultMethodCallWrapper();
            var type = module.DefineType("TestType");

            var fooMethod = typeof(IFoo).GetMethod("Foo");

            var field = type.DefineField("_Foo_", typeof(Func<string>), FieldAttributes.Public);

            // Act
            wrapper.GenerateInvocation(type, typeof(IFoo),
                fooMethod,
                new[] { field });

            type.AddInterfaceImplementation(typeof(IFoo));
            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo_").SetValue(obj, new Func<string>(Foo));
            var result = fooMethod.Invoke(obj, null);
            Assert.AreEqual("Hello World", result);
        }

        public static string Foo_WithString(string argument)
        {
            return string.Format("Hello {0}!", argument);
        }

        [Test]
        public void GenerateInvocation_OneParameters_Ok()
        {
            // Arrange
            var wrapper = new DefaultMethodCallWrapper();
            var type = module.DefineType(Guid.NewGuid().ToString());
            type.AddInterfaceImplementation(typeof(IFooWithString));
            var fooString = typeof(IFooWithString).GetMethod("Foo_WithString");

            var field = type.DefineField("_Foo_WithString_String", typeof(Func<string, string>), FieldAttributes.Public);

            // Act
            var resultMethod = wrapper.GenerateInvocation(type, typeof(IFooWithString),
                fooString,
                new[] { field });

            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo_WithString_String").SetValue(obj, new Func<string, string>(Foo_WithString));
            var result = fooString.Invoke(obj, new object[] { "World" });

            Assert.AreEqual("Hello World!", result);
        }


    }
}
