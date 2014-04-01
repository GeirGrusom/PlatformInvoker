using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    /// <summary>
    /// Provides an implementation for a <see cref="ConstructorBuilder"/> that emits functionality for <see cref="IMethodCallProbe{TInterface}"/>.
    /// </summary>
    public class ProbingConstructorBuilder : DefaultConstructorBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lookupFunctionName">Supplies a function lookup name transformation. Set this to null to use the method name verbatim.</param>
        public ProbingConstructorBuilder(Func<string, string> lookupFunctionName)
            : base(lookupFunctionName)
        {
            
        }

        /// <summary>
        /// The field created for the <see cref="IMethodCallProbe{TInterface}"/> instance.
        /// </summary>
        public FieldBuilder ProbeField { get; private set; }


        /// <summary>
        /// Defines the constructor used.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        protected override ConstructorBuilder DefineConstructor(TypeBuilder owner, Type interfaceType)
        {
            var probeType = typeof(IMethodCallProbe<>).MakeGenericType(interfaceType);
            return owner.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(ILibrary), probeType });
        }

        /// <summary>
        /// Emits begin call
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <param name="generator"></param>
        protected override void EmitBegin(TypeBuilder type, Type interfaceType, ILGenerator generator)
        {

            var probeType = typeof(IMethodCallProbe<>).MakeGenericType(interfaceType);

            var field = type.DefineField("$probe", probeType, FieldAttributes.Private | FieldAttributes.InitOnly);
            ProbeField = field;
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Stfld, field);

            base.EmitBegin(type, interfaceType, generator);
        }
    }
}