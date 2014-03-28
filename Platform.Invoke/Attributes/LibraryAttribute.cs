using System;

namespace Platform.Invoke.Attributes
{
    /// <summary>
    /// Defines a format for entry point lookup names in the library implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class EntryPointFormatAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the formatting to use for name lookups. Use {0} to specify method name.
        /// </summary>
        public string Format;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format">Format to use. {0} defines the member name.</param>
        public EntryPointFormatAttribute(string format)
        {
            this.Format = format;
        }
    }

    /// <summary>
    /// Explicitly states the library to use.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public class LibraryAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the library to use.
        /// </summary>
        public string Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public LibraryAttribute(string name)
        {
            this.Name = name;
        }
    }
}