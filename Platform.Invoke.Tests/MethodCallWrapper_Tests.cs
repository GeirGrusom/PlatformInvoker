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
        public void Test()
        {
            // Arrange
            var wrapper = new MethodCallWrapper(f => "_" + f.Name);
            var type = module.DefineType("TestType");

            var method = type.DefineMethod("Foo", MethodAttributes.Public);
            var field = type.DefineField("_Foo", typeof (Func<string>), FieldAttributes.Private);

            // Act
            wrapper.GenerateInvocation(method.GetILGenerator(), 
                GetType().GetMethod("Foo", BindingFlags.Static | BindingFlags.Public),
                new [] {field});

            var resultType = type.CreateType();

            // Assert

            var obj = Activator.CreateInstance(resultType);

            obj.GetType().GetMethod("Foo").Invoke(obj, new object[0]);



        }

    }
}
