using System;

namespace Platform.Invoke.Attributes
{
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