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
    public class ProbingConstructorBuilder_Tests
    {

        private AssemblyBuilder assembly;
        private ModuleBuilder module;

        public interface IFoo
        {
            string Foo();
        }

        [SetUp]
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

            var builder = new ProbingConstructorBuilder(null);
            var lib = Substitute.For<ILibrary>();
            var probe = Substitute.For<IMethodCallProbe<IFoo>>();

            var f = type.DefineField("_Foo", typeof(Func<string>), FieldAttributes.Private | FieldAttributes.InitOnly);

            // Act
            var ctor = builder.GenerateConstructor(type, typeof(IFoo), new MethodInfo[0], new[] { f });

            // Assert
            var result = Activator.CreateInstance(type.CreateType(), lib, probe);

            Assert.IsNotNull(result);
            var probeField = result.GetType().GetField("$probe", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(probeField);
            Assert.AreEqual(probe, probeField.GetValue(result));
        }
    }
}
