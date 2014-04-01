using System;
using System.Runtime.Serialization;

namespace Platform.Invoke
{
    /// <summary>
    /// Indicates an exception where a entry point could not be found in a library.
    /// </summary>
    [Serializable]
    public sealed class MissingEntryPointException : Exception
    {
        /// <summary>
        /// Gets the name of the entry point.
        /// </summary>
        public string EntryPoint { get; private set; }

        /// <summary>
        /// Gets the name of the lookup library.
        /// </summary>
        public string LibraryName { get; private set; }

        private MissingEntryPointException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EntryPoint = info.GetString("EntryPoint");
            LibraryName = info.GetString("LibraryName");
        }

        /// <summary>
        /// Creates an instance of an exception describing a missing entry point.
        /// </summary>
        /// <param name="entryPoint"></param>
        /// <param name="library"></param>
        public MissingEntryPointException(string entryPoint, ILibrary library)
            : base(string.Format("The entry point '{0}' could not be found in the library '{1}'.", entryPoint, library != null ? library.Name : null))
        {
            EntryPoint = entryPoint;
            LibraryName = library != null ? library.Name : null;
        }
    }
}
