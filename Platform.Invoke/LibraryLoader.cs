using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Invoke
{
    public interface ILibrary : IDisposable
    {
        string Name { get; }

        [Pure]
        Delegate GetProcedure(Type delegateType, string name);

        [Pure]
        TDelegate GetProcedure<TDelegate>(string name)
            where TDelegate : class; // Can't constraint to Delegate for some reason...
    }

    public interface ILibraryLoader
    {
        [Pure]
        ILibrary Load(string libraryName);
    }
}
