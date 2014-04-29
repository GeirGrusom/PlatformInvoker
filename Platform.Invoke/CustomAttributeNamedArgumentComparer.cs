using System.Collections.Generic;
using System.Reflection;

namespace Platform.Invoke
{
    internal class CustomAttributeNamedArgumentComparer : IEqualityComparer<CustomAttributeNamedArgument>
    {
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