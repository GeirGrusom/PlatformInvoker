using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
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

        public interface IFooWithString
        {
            string Foo_WithString(string arg);
        }

        [TestFixtureSetUp]
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

        [Test]
        public void GenerateInvocation_NoParameters_Ok()
        {
            // Arrange
            var wrapper = new DefaultMethodCallWrapper(f => "_" + f.Name);
            var type = module.DefineType("TestType");
            
            var fooMethod = typeof(IFoo).GetMethod("Foo");
            
            var field = type.DefineField("_Foo", typeof (Func<string>), FieldAttributes.Public);

            // Act
            wrapper.GenerateInvocation(type, 
                fooMethod,
                new [] {field});

            type.AddInterfaceImplementation(typeof(IFoo));
            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo").SetValue(obj, new Func<string>(Foo));
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
            var wrapper = new DefaultMethodCallWrapper(f => "_" + f.Name);
            var type = module.DefineType(Guid.NewGuid().ToString());
            type.AddInterfaceImplementation(typeof(IFooWithString));
            var fooString = typeof(IFooWithString).GetMethod("Foo_WithString");

            var field = type.DefineField("_Foo_WithString", typeof(Func<string, string>), FieldAttributes.Public);

            // Act
            var resultMethod = wrapper.GenerateInvocation(type,
                fooString,
                new[] { field });

            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo_WithString").SetValue(obj, new Func<string, string>(Foo_WithString));
            var result = fooString.Invoke(obj, new object[] { "World" });

            Assert.AreEqual("Hello World!", result);



        }


    }
}
