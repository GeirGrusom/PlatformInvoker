using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Invoke
{
    public class LibraryInterfaceMapper
    {
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly IDelegateTypeBuilder delegateBuilder;
        public LibraryInterfaceMapper(IDelegateTypeBuilder delegateBuilder)
        {
            this.delegateBuilder = delegateBuilder;
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DynamicInterfaces"),
                AssemblyBuilderAccess.Run);

            moduleBuilder = assemblyBuilder.DefineDynamicModule("InterfaceMapping");
        }

        public TInterface Implement<TInterface>(ILibrary library)
            where TInterface : class
        {
            var type = typeof (TInterface);
            if(!type.IsInterface)
                throw new ArgumentException("TInterface must be a...interface...type...");

            var definedType = moduleBuilder.DefineType(string.Format("{0}_Implementation", typeof(TInterface).Name),
                                         TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);

            var methods = type.GetMethods(BindingFlags.Default);

            var constructor = definedType.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.Final,
                CallingConventions.HasThis, new [] { typeof(ILibrary) });

            var fields = GenerateFields(methods, moduleBuilder, definedType);

            return null;
        }

        private static void GenerateConstructor(ConstructorBuilder builder, IEnumerable<MethodInfo> methods, IEnumerable<FieldBuilder> fields, string extensionMethodPrefix)
        {
            var generator = builder.GetILGenerator();
            generator.DeclareLocal(typeof(Delegate));
            var getMethod = typeof(ILibrary).GetMethod("GetProcedure", new[] { typeof(string), typeof(Type) });
            var notSupportedConstructor = typeof(MissingMethodException).GetConstructor(
                new[] { typeof(string), typeof(string) });
            if (notSupportedConstructor == null) // Constructor required. Added to make unit tests fail if it is missing.
                throw new MissingMethodException("ExtensionNotSupportedException", ".ctr(string)");

            var fieldBuilders = fields as FieldBuilder[] ?? fields.ToArray();
            foreach (var method in methods)
            {
                string methodName = (extensionMethodPrefix ?? "") + method.Name;
                var name = GetFieldNameForMethodInfo(method);
                var field = fieldBuilders.Single(f => f.Name == name);
                var okLabel = generator.DefineLabel();

                // _glMethodName = (MethodDelegateType)extensionSupport.GetProcedure("glMethodName", typeof(MethodDelegateType));
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldstr, methodName);  // load method name
                generator.Emit(OpCodes.Ldtoken, field.FieldType); // load field type
                generator.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }), null);
                generator.EmitCall(OpCodes.Callvirt, getMethod, null);
                generator.Emit(OpCodes.Stloc_0); // result = GetProcedure("MethodName", Type);
                // if result == null throw ExtensionNotSupportedException
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Brtrue, okLabel);
                generator.Emit(OpCodes.Ldstr, methodName);
                generator.Emit(OpCodes.Newobj, notSupportedConstructor);
                generator.Emit(OpCodes.Throw);
                generator.MarkLabel(okLabel);
                // Everything went okay. Set the delegate to the returned function.
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Castclass, field.FieldType);
                generator.Emit(OpCodes.Ldarg_0); // this
                generator.Emit(OpCodes.Ldloc_0); // result
                generator.Emit(OpCodes.Stfld, field); // this._fieldName = result;
            }
            generator.Emit(OpCodes.Ret);
        }

        private IList<FieldBuilder> GenerateFields(IEnumerable<MethodInfo> methods, ModuleBuilder delegateModule, TypeBuilder builder)
        {
            return (from method in methods
                    let delegateType = delegateBuilder.CreateDelegateType(method, delegateModule)
                    select builder.DefineField(GetFieldNameForMethodInfo(method), 
                    delegateType, 
                    FieldAttributes.Private | FieldAttributes.InitOnly)).ToList();
        }

        private static string GetFieldNameForMethodInfo(MethodInfo method)
        {
            return string.Format("_{0}", method.Name);
        }
    }
}
