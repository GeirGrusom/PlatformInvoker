using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Platform.Invoke.Attributes;

namespace Platform.Invoke
{
    /// <summary>
    /// Provides an interface for constructor builder implementers.
    /// </summary>
    public interface IConstructorBuilder
    {
        /// <summary>
        /// Generates the constructor using the specified arguments.
        /// </summary>
        /// <param name="owner">Constructor owner type</param>
        /// <param name="interfaceType">Type of interface implemented by <see paramref="owner"/>.</param>
        /// <param name="methods">Methods defined by <see paramref="owner"/> that exposes the functions defined by <see paramref="interfaceType"/>.</param>
        /// <param name="fields">Internal fields for function delegates defined by <see paramref="owner"/>.</param>
        /// <returns><see cref="ConstructorBuilder"/> for the specified <see paramref="owner"/>.</returns>
        ConstructorBuilder GenerateConstructor(
            TypeBuilder owner,
            Type interfaceType,
            IEnumerable<MethodInfo> methods,
            IEnumerable<FieldBuilder> fields);
    }

    /// <summary>
    /// Defines the default implementation of IConstructorBuilder.
    /// </summary>
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
            this.lookupFunctionName = lookupFunctionName;
        }

        /// <summary>
        /// Creates the constructor builder.
        /// </summary>
        /// <param name="owner">Owner type to create constructor for.</param>
        /// <param name="interfaceType"></param>
        /// <returns><see cref="ConstructorBuilder" /> specified in <see paramref="owner"/>.</returns>
        protected virtual ConstructorBuilder DefineConstructor(TypeBuilder owner, Type interfaceType)
        {
            return owner.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(ILibrary) });
        }


        /// <summary>
        /// Method is invoked at start of constructor generator.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <param name="generator"></param>
        protected virtual void EmitBegin(TypeBuilder type, Type interfaceType, ILGenerator generator)
        {

        }

        /// <summary>
        /// Method is invoked at the end of the constructor generator.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <param name="generator"></param>
        protected virtual void EmitEnd(TypeBuilder type, Type interfaceType, ILGenerator generator)
        {

        }

        /// <summary>
        /// Generates the constructor using the specified arguments.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="interfaceType"></param>
        /// <param name="methods"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public ConstructorBuilder GenerateConstructor(TypeBuilder owner, Type interfaceType, IEnumerable<MethodInfo> methods, IEnumerable<FieldBuilder> fields)
        {
            var builder = DefineConstructor(owner, interfaceType);

            var generator = builder.GetILGenerator();

            var notSupportedConstructor = typeof(MissingEntryPointException).GetConstructor(
                new[] { typeof(string), typeof(ILibrary) });

            if (notSupportedConstructor == null)
                throw new MissingMethodException("MissingEntryPointException", ".ctr(string, ILibrary)");
            var loc = generator.DeclareLocal(typeof(Delegate));
            var fieldBuilders = fields as FieldBuilder[] ?? fields.ToArray();

            EmitBegin(owner, interfaceType, generator);

            foreach (var method in methods)
            {
                generator.BeginScope();

                var entryPoint = method.GetCustomAttributes(typeof(EntryPointAttribute), false).OfType<EntryPointAttribute>().FirstOrDefault();

                string methodName;
                if (entryPoint != null)
                {
                    methodName = entryPoint.Name;
                }
                else if (lookupFunctionName != null)
                {
                    methodName = lookupFunctionName(method.Name);
                }
                else
                {
                    var libAttrib = interfaceType.GetCustomAttributes(typeof(EntryPointFormatAttribute), false).OfType<EntryPointFormatAttribute>().FirstOrDefault();
                    if (libAttrib != null && !string.IsNullOrEmpty(libAttrib.Format))
                        methodName = string.Format(libAttrib.Format, method.Name);
                    else
                        methodName = method.Name;
                }

                var name = LibraryInterfaceMapper.GetFieldNameForMethodInfo(method);
                var field = fieldBuilders.Single(f => f.Name == name);
                var okLabel = generator.DefineLabel();

                generator.Emit(OpCodes.Ldarg_1); // ILibrary
                generator.Emit(OpCodes.Ldstr, methodName);  // load constant method name

                var getMethod = typeof(ILibrary).GetMethod(nameof(ILibrary.GetProcedure), new[] { typeof(string) }).MakeGenericMethod(field.FieldType); // lib.GetProcedure<Field.FieldType>(methodName)
                generator.EmitCall(OpCodes.Callvirt, getMethod, null);

                generator.Emit(OpCodes.Stloc, loc); // result = GetProcedure<DelegateType>(methodName);

                // if result == null throw MethodNotSupportedException
                generator.Emit(OpCodes.Ldloc, loc);
                generator.Emit(OpCodes.Brtrue, okLabel); // if(result != null) goto okLabel
                generator.Emit(OpCodes.Ldstr, methodName);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Newobj, notSupportedConstructor);
                generator.Emit(OpCodes.Throw); // throw new MissingMethodException(methodName)

                generator.MarkLabel(okLabel);
                // Everything went okay. Set the delegate to the returned function.
                generator.Emit(OpCodes.Ldarg_0); // this
                generator.Emit(OpCodes.Ldloc, loc); // result
                generator.Emit(OpCodes.Stfld, field); // this._fieldName = result;

                generator.EndScope();
            }

            EmitEnd(owner, interfaceType, generator);

            generator.Emit(OpCodes.Ret);

            return builder;
        }
    }
}