using System;
using System.Reflection;
using System.Reflection.Emit;

using NSubstitute;

using NUnit.Framework;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    public class ProbingMethodCallWrapper_Tests
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
            module = assembly.DefineDynamicModule("TestModule", true);
        }

        private static string Foo()
        {
            return "Hello World!";
        }

        [Test]
        public void InvokesBegin()
        {
            // Arrange
            var probe = Substitute.For<IMethodCallProbe<IFoo>>();

            // Setup mock type builder
            var type = module.DefineType("TestType");
            var foo = type.DefineField("_Foo_", typeof(Func<string>), FieldAttributes.Private);
            var probeField = type.DefineField("$probe", typeof(IMethodCallProbe<IFoo>), FieldAttributes.Private);
            type.DefineDefaultConstructor(MethodAttributes.Public);
            type.AddInterfaceImplementation(typeof(IFoo));
            var wrapper = new ProbingMethodCallWrapper(() => probeField);
            wrapper.GenerateInvocation(type, typeof(IFoo), typeof(IFoo).GetMethod("Foo"), new[] { foo });
            var resultType = type.CreateType();
            var result = Activator.CreateInstance(resultType);
            result.GetType().GetField("$probe", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(result, probe);
            result.GetType().GetField("_Foo_", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(result, new Func<string>(Foo));

            // Act
            string res = ((IFoo)result).Foo();

            // Assert
            Assert.AreEqual("Hello World!", res);
            probe.Received().OnBeginInvoke(Arg.Any<MethodInfo>(), (IFoo)result);
            probe.Received().OnEndInvoke(Arg.Any<MethodInfo>(), (IFoo)result);
        }
    }
}
