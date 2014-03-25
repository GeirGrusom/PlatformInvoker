using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using NSubstitute;

using NUnit.Framework;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    class DefaultConstructorBuilder_Tests
    {

        private AssemblyBuilder assembly;
        private ModuleBuilder module;

        private interface IFoo
        {
            string Foo();
        }

        private interface IFooBar
        {
            string Foo();
            string Bar();
        }


        [TestFixtureSetUp]
        public void Setup()
        {
            assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("TestAssembly"),
                AssemblyBuilderAccess.RunAndCollect);
            module = assembly.DefineDynamicModule("TestModule", emitSymbolInfo: true);
        }

        [Test]
        public void GeneratesConstructor_Ok()
        {
            // Arrange
            var type = module.DefineType("ConstructorType", TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout | TypeAttributes.Public);
            var builder = new DefaultConstructorBuilder();
            var lib = Substitute.For<ILibrary>();
            lib.GetProcedure(typeof(Func<string>), "Foo").Returns(new Func<string>(() => "Hello world!"));
            lib.GetProcedure<Func<string>>("Foo").Returns(() => "Hello world!");

            
            var f = type.DefineField("_Foo", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);
            
            // Act
            var ctor = builder.GenerateConstructor(type, typeof(IFoo).GetMethods(), new[] { f }, "");

            // Assert
            var result = Activator.CreateInstance(type.CreateType(), lib);
            
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.GetType().GetField("_Foo", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(result));
        }

        [Test]
        public void GeneratesConstructor_TwoArguments_Ok()
        {
            // Arrange
            var type = module.DefineType("ConstructorType_TwoArguments_Ok", TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout | TypeAttributes.Public);
            var builder = new DefaultConstructorBuilder();
            var lib = Substitute.For<ILibrary>();
            
            lib.GetProcedure<Func<string>>("Foo").Returns(() => "Hello world!");
            lib.GetProcedure<Func<string>>("Bar").Returns(() => "Hello world!");


            var foo = type.DefineField("_Foo", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);
            var bar = type.DefineField("_Bar", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);

            // Act
            var ctor = builder.GenerateConstructor(type, typeof(IFooBar).GetMethods(), new[] { foo, bar }, "");

            // Assert
            var result = Activator.CreateInstance(type.CreateType(), lib);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.GetType().GetField("_Foo", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(result));
            Assert.IsNotNull(result.GetType().GetField("_Bar", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(result));
        }

    }
}
