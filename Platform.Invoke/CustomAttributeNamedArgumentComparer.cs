using System.Collections.Generic;
using System.Reflection;

namespace Platform.Invoke
{
    internal sealed class CustomAttributeNamedArgumentComparer : IEqualityComparer<CustomAttributeNamedArgument>
    {
        public static readonly CustomAttributeNamedArgumentComparer Instance = new CustomAttributeNamedArgumentComparer();

        public bool Equals(CustomAttributeNamedArgument x, CustomAttributeNamedArgument y)
        {
            return x.MemberInfo.Name == y.MemberInfo.Name;
        }

        public int GetHashCode(CustomAttributeNamedArgument obj)
        {
            return obj.MemberInfo.Name.GetHashCode();
        }
    }
}