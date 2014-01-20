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
    public class MethodCallWrapper_Tests
    {
        private AssemblyBuilder assembly;
        private ModuleBuilder module;

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
            var wrapper = new MethodCallWrapper(f => "_" + f.Name);
            var type = module.DefineType("TestType");

            var method = type.DefineMethod("Foo", MethodAttributes.Public);
            var field = type.DefineField("_Foo", typeof (Func<string>), FieldAttributes.Public);

            // Act
            wrapper.GenerateInvocation(method.GetILGenerator(), 
                GetType().GetMethod("Foo", BindingFlags.Static | BindingFlags.Public),
                new [] {field});

            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo").SetValue(obj, new Func<string>(Foo));
            obj.GetType().GetMethod("Foo").Invoke(obj, new object[0]);



        }

        public static string Foo_WithString(string argument)
        {
            return string.Format("Hello {0}!", argument);
        }

        [Test]
        public void GenerateInvocation_OneParameters_Ok()
        {
            // Arrange
            var wrapper = new MethodCallWrapper(f => "_" + f.Name);
            var type = module.DefineType(Guid.NewGuid().ToString());

            var method = type.DefineMethod("Foo_WithString", MethodAttributes.Public, typeof(string), new[] { typeof(string) });

            var field = type.DefineField("_Foo_WithString", typeof(Func<string, string>), FieldAttributes.Public);

            // Act
            wrapper.GenerateInvocation(method.GetILGenerator(),
                GetType().GetMethod("Foo_WithString", BindingFlags.Static | BindingFlags.Public, null, new [] { typeof(string) },  null ),
                new[] { field });

            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);
            obj.GetType().GetField("_Foo_WithString").SetValue(obj, new Func<string, string>(Foo_WithString));
            var result = obj.GetType().GetMethod("Foo_WithString").Invoke(obj, new object[] { "World" });

            Assert.AreEqual("Hello World!", result);



        }


    }
}
