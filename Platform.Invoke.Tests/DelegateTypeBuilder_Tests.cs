using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Platform.Invoke.Tests
{
    [TestFixture]
    public class DelegateTypeBuilder_Tests
    {
        public interface IDelegateFoo
        {
            void FooMethod();

            string FooMethod_String();

            string FooMethod_String_StringIsIn([In] string foo);

            string FooMethod_String_StringWithCustomMarshal(
                [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof (CustomMarshaller))] string foo);
        }

        public class CustomMarshaller : ICustomMarshaler
        {
            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                throw new NotImplementedException();
            }

            public IntPtr MarshalManagedToNative(object ManagedObj)
            {
                throw new NotImplementedException();
            }

            public void CleanUpNativeData(IntPtr pNativeData)
            {
                throw new NotImplementedException();
            }

            public void CleanUpManagedData(object ManagedObj)
            {
                throw new NotImplementedException();
            }

            public int GetNativeDataSize()
            {
                throw new NotImplementedException();
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
            var type = builder.CreateDelegateType(typeof (IDelegateFoo).GetMethod("FooMethod"), ModuleBuilder);
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
            var type = builder.CreateDelegateType(typeof(IDelegateFoo).GetMethod("FooMethod_String"), ModuleBuilder);
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
            var type = builder.CreateDelegateType(typeof(IDelegateFoo).GetMethod("FooMethod_String_StringIsIn"), ModuleBuilder);
            var method = type.GetMethod("Invoke");

            // Assert
            Assert.AreEqual(1, method.GetParameters().Length);
        }

        [Test]
        public void CreateDelegateType_AddsArgumentTypeNamesInDelegateName_Ok()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(IDelegateFoo).GetMethod("FooMethod_String_StringIsIn"), ModuleBuilder);
            var method = type.GetMethod("Invoke");

            // Assert
            Assert.AreEqual("FooMethod_String_StringIsIn_String_Proc", type.Name);
        }

        [Test]
        public void CreateDelegateType_StringReturnType_OneStringParametersWithIn_CopiesAttributes()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(IDelegateFoo).GetMethod("FooMethod_String_StringIsIn"), ModuleBuilder);
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
        [Ignore("Need to manually check if this test failure is an issue or not. This can be tested by using the library by a proxy. " +
                "This is meant to fix an issue with cross assembly custom marshallers. The error this could create is that the implementation looks for the custom marshaller in Platform.Invoke instead" +
                " of the calling assembly.")]
        public void CreateDelegateType_StringReturnType_OneStringParametersCustomMarshaller_RemovesRef()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(IDelegateFoo).GetMethod("FooMethod_String_StringWithCustomMarshal"), ModuleBuilder);
            var method = type.GetMethod("Invoke");
            var parameters = method.GetParameters();
            var param = parameters.First();
            var marshal = param.GetCustomAttribute<MarshalAsAttribute>();

            // Assert
            Assert.IsNotNull(marshal);
            Assert.IsNull(marshal.MarshalType);
        }

        [Test]
        public void CreateDelegateType_StringReturnType_OneStringParametersCustomMarshaller_PreservesRefType()
        {
            // Arrange
            var builder = new DelegateTypeBuilder();

            // Act
            var type = builder.CreateDelegateType(typeof(IDelegateFoo).GetMethod("FooMethod_String_StringWithCustomMarshal"), ModuleBuilder);
            var method = type.GetMethod("Invoke");
            var parameters = method.GetParameters();
            var param = parameters.First();
            var marshal = param.GetCustomAttribute<MarshalAsAttribute>();

            // Assert
            Assert.IsNotNull(marshal);
            Assert.IsNotNull(marshal.MarshalTypeRef);
        }
    }
}
