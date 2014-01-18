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
        public TInterface Implement<TInterface>(ILibrary library)
            where TInterface : class
        {
            var type = typeof (TInterface);
            if(!type.IsInterface)
                throw new ArgumentException("TInterface must be a...interface...type...");

            return null;
        }

        private static string GetFieldNameForMethodInfo(MethodInfo method)
        {
            return string.Format("_{0}", method.Name);
        }
    }
}
