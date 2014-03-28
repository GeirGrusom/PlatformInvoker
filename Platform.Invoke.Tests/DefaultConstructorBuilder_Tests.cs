using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using NSubstitute;

using NUnit.Framework;

using Platform.Invoke.Attributes;

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

        private interface IFooWithEntryPoint
        {
            [EntryPoint("Bar")]
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
        public void UsesEntryPointAttributeForLookup_FieldNameRemainsTheSame()
        {
            // Arrange
            var type = module.DefineType("ConstructorType_FieldRemainsTheSame", TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout | TypeAttributes.Public);
            var builder = new DefaultConstructorBuilder(null);
            var lib = Substitute.For<ILibrary>();
            lib.GetProcedure<Func<string>>(Arg.Any<string>()).Returns(() => "Hello world!");

            var f = type.DefineField("_Foo_", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);

            // Act
            var ctor = builder.GenerateConstructor(type, typeof(IFooWithEntryPoint), typeof(IFooWithEntryPoint).GetMethods(), new[] { f });
            var result = Activator.CreateInstance(type.CreateType(), lib);

            // Assert
            lib.Received().GetProcedure<Func<string>>("Bar");
        }

        [Test]
        public void GeneratesConstructor_Ok()
        {
            // Arrange
            var type = module.DefineType("ConstructorType_Ok", TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout | TypeAttributes.Public);
            var builder = new DefaultConstructorBuilder(null);
            var lib = Substitute.For<ILibrary>();
            lib.GetProcedure<Func<string>>("Foo").Returns(() => "Hello world!");

            
            var f = type.DefineField("_Foo_", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);
            
            // Act
            var ctor = builder.GenerateConstructor(type, typeof(IFoo), typeof(IFoo).GetMethods(), new[] { f });

            // Assert
            var result = Activator.CreateInstance(type.CreateType(), lib);
            
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.GetType().GetField("_Foo_", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(result));
        }

        [Test]
        public void GeneratesConstructor_TwoArguments_Ok()
        {
            // Arrange
            var type = module.DefineType("ConstructorType_TwoArguments_Ok", TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout | TypeAttributes.Public);
            var builder = new DefaultConstructorBuilder(null);
            var lib = Substitute.For<ILibrary>();
            
            lib.GetProcedure<Func<string>>("Foo").Returns(() => "Hello world!");
            lib.GetProcedure<Func<string>>("Bar").Returns(() => "Hello world!");


            var foo = type.DefineField("_Foo_", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);
            var bar = type.DefineField("_Bar_", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);

            // Act
            var ctor = builder.GenerateConstructor(type,  typeof(IFooBar), typeof(IFooBar).GetMethods(), new[] { foo, bar });

            // Assert
            var result = Activator.CreateInstance(type.CreateType(), lib);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.GetType().GetField("_Foo_", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(result));
            Assert.IsNotNull(result.GetType().GetField("_Bar_", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(result));
        }
    }
}
