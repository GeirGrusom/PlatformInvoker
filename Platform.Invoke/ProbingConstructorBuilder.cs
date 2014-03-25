using System.Reflection;
using System.Reflection.Emit;

namespace Platform.Invoke
{
    public class ProbingConstructorBuilder : DefaultConstructorBuilder
    {

        public FieldBuilder ProbeField { get; private set; }

        protected override ConstructorBuilder DefineConstructor(TypeBuilder owner)
        {
            return owner.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(ILibrary), typeof(IMethodCallProbe) });
        }


        protected override void EmitBegin(TypeBuilder type, ILGenerator generator)
        {
            var field = type.DefineField("$probe", typeof(IMethodCallProbe), FieldAttributes.Private | FieldAttributes.InitOnly);
            ProbeField = field;
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Stfld, field);

            base.EmitBegin(type, generator);
        }
    }
}