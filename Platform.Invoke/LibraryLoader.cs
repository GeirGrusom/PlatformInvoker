using System;
using System.Diagnostics.Contracts;

namespace Platform.Invoke
{
    /// <summary>
    /// Specifies an interface for libraries.
    /// </summary>
    public interface ILibrary : IDisposable
    {
        /// <summary>
        /// Gets the name of the library.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a procedure for the named function and returns it as the specified delegate type.
        /// </summary>
        /// <param name="delegateType">Type of <see cref="Delegate"/> to return.</param>
        /// <param name="name">Function to return a delegate for.</param>
        /// <returns>A delegate if the function is found. Returns null if none can be found.</returns>
        [Pure]
        Delegate GetProcedure(Type delegateType, string name);

        /// <summary>
        /// Gets a procedure for the named function and returns it as the specified delegate type.
        /// </summary>
        /// <typeparam name="TDelegate">Delegate type to return.</typeparam>
        /// <param name="name">Function to return a delegate for.</param>
        /// <returns>A delegate if the function is found. Returns null if none can be found.</returns>
        [Pure]
        TDelegate GetProcedure<TDelegate>(string name)
            where TDelegate : class; // Can't constraint to Delegate for some reason...
    }

    /// <summary>
    /// Specifies an interface for library loaders.
    /// </summary>
    public interface ILibraryLoader
    {
        /// <summary>
        /// Loads a library and returns an wrapper implementation for loading functions from it.
        /// </summary>
        /// <param name="libraryName">Name of library to load.</param>
        /// <returns>Returns a library function loader wrapper implementation.</returns>
        [Pure]
        ILibrary Load(string libraryName);
    }
}
