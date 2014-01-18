using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    public class DelegateTypeBuilder_Tests
    {
        private class DelegateFoo
        {
            public void FooMethod()
            {
                
            }

            public string FooMethod_String()
            {
                return null;
            }

            public string FooMethod_String_StringIsIn([In]string foo)
            {
                return null;
            }

            public string FooMethod_String_StringWithCustomMarshal([MarshalAs(UnmanagedType.R8, MarshalTypeRef = typeof(string))]string foo)
            {
                return null;
            }

        }

        private static readonly AssemblyBuilder AssemblyBuilder =
            System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("TestAssembly"),
                AssemblyBuilderAccess.Run);

        private static readonly ModuleBuilder ModuleBuilder = AssemblyBuilder.DefineDynamicModule("TestModule");

        [Test]
        public void CreateDelegateType_VoidReturnType_NoParameters_Ok()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof (DelegateFoo).GetMethod("FooMethod"), ModuleBuilder);
            var method = type.GetMethod("Invoke");

            // Assert
            Assert.That(typeof(Delegate).IsAssignableFrom(type));
            Assert.AreEqual(typeof(void), method.ReturnType);
        }

        [Test]
        public void CreateDelegateType_StringReturnType_NoParameters_Ok()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(DelegateFoo).GetMethod("FooMethod_String"), ModuleBuilder);
            var method = type.GetMethod("Invoke");

            // Assert
            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(string), method.ReturnType);
        }

        [Test]
        public void CreateDelegateType_StringReturnType_OneStringParameters_Ok()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(DelegateFoo).GetMethod("FooMethod_String_String"), ModuleBuilder);
            var method = type.GetMethod("Invoke");

            // Assert
            Assert.AreEqual(1, method.GetParameters().Length);
        }

        [Test]
        public void CreateDelegateType_StringReturnType_OneStringParametersWithIn_CopiesAttributes()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(DelegateFoo).GetMethod("FooMethod_String_StringIsIn"), ModuleBuilder);
            var method = type.GetMethod("Invoke");
            var parameters = method.GetParameters();
            var param = parameters.First();

            // Assert
            Assert.IsNotNull(param.GetCustomAttribute<InAttribute>());
        }

        /// <summary>
        /// This test checks that the MarshalType field is removed from MarshalAsAttribute.
        /// The reason for this is that MarshalAsAttribute will set MarshalType even if only MarshalTypeRef is set.
        /// This will make any application using a custom marshaller fail since it will search for the type by
        /// name in this assembly rather than the assembly containing the calling code.
        /// </summary>
        [Test]
        public void CreateDelegateType_StringReturnType_OneStringParametersCustomMarshaller_RemovesRef()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(DelegateFoo).GetMethod("FooMethod_String_StringWithCustomMarshal"), ModuleBuilder);
            var method = type.GetMethod("Invoke");
            var parameters = method.GetParameters();
            var param = parameters.First();
            var marshal = param.GetCustomAttribute<MarshalAsAttribute>();

            // Assert
            Assert.IsNotNull(marshal);
            Assert.IsNull(marshal.MarshalType);
        }
    }
}
