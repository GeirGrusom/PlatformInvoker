using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    public interface IConstructorBuilder
    {
        ConstructorBuilder GenerateConstructor(TypeBuilder owner,
            IEnumerable<MethodInfo> methods,
            IEnumerable<FieldBuilder> fields);
    }

    [ImmutableObject(true)]
    public class DefaultConstructorBuilder : IConstructorBuilder
    {
        private readonly Func<string, string> lookupFunctionName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lookupFunctionName">Supplies a function lookup name transformation. Set this to null to use the method name verbatim.</param>
        public DefaultConstructorBuilder(Func<string, string> lookupFunctionName)
        {
            if (lookupFunctionName == null)
                this.lookupFunctionName = s => s;
            else
                this.lookupFunctionName = lookupFunctionName;
        }


        protected virtual ConstructorBuilder DefineConstructor(TypeBuilder owner)
        {
            return owner.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(ILibrary) });
        }


        protected virtual void EmitBegin(TypeBuilder type, ILGenerator generator)
        {
            
        }


        protected virtual void EmitEnd(TypeBuilder type, ILGenerator generator)
        {
            
        }

        public ConstructorBuilder GenerateConstructor(TypeBuilder owner, IEnumerable<MethodInfo> methods, IEnumerable<FieldBuilder> fields)
        {
            var builder = DefineConstructor(owner);

            var generator = builder.GetILGenerator();
            
            var notSupportedConstructor = typeof(MissingMethodException).GetConstructor(
                new[] { typeof(string) });

            if (notSupportedConstructor == null)
                throw new MissingMethodException("MissingMethodException", ".ctr(string)");
            var loc = generator.DeclareLocal(typeof(Delegate));
            var fieldBuilders = fields as FieldBuilder[] ?? fields.ToArray();

            EmitBegin(owner, generator);

            foreach (var method in methods)
            {
                generator.BeginScope();
                
                string methodName = lookupFunctionName(method.Name);
                var name = LibraryInterfaceMapper.GetFieldNameForMethodInfo(method);
                var field = fieldBuilders.Single(f => f.Name == name);
                var okLabel = generator.DefineLabel();

                generator.Emit(OpCodes.Ldarg_1); // ILibrary
                generator.Emit(OpCodes.Ldstr, methodName);  // load constant method name

                var getMethod = typeof(ILibrary).GetMethod("GetProcedure", new[] { typeof(string) }).MakeGenericMethod(field.FieldType); // lib.GetProcedure<Field.FieldType>(methodName)
                generator.EmitCall(OpCodes.Callvirt, getMethod, null);

                generator.Emit(OpCodes.Stloc, loc); // result = GetProcedure<DelegateType>(methodName);
                
                // if result == null throw MethodNotSupportedException
                generator.Emit(OpCodes.Ldloc, loc); 
                generator.Emit(OpCodes.Brtrue_S, okLabel); // if(result != null) goto okLabel
                generator.Emit(OpCodes.Ldstr, methodName);
                generator.Emit(OpCodes.Newobj, notSupportedConstructor);
                generator.Emit(OpCodes.Throw); // throw new MissingMethodException(methodName)
                
                generator.MarkLabel(okLabel);
                // Everything went okay. Set the delegate to the returned function.
                generator.Emit(OpCodes.Ldarg_0); // this
                generator.Emit(OpCodes.Ldloc, loc); // result
                generator.Emit(OpCodes.Stfld, field); // this._fieldName = result;
                
                generator.EndScope();
            }

            EmitEnd(owner, generator);

            generator.Emit(OpCodes.Ret);

            return builder;
        }
    }
}