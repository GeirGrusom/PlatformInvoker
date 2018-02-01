using System;

namespace Platform.Invoke.Attributes
{
    /// <summary>
    /// Explicitly defines the entry point to use for the method call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EntryPointAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the entry point name.
        /// </summary>
        public string Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public EntryPointAttribute(string name)
        {
            this.Name = name;
        }
    }
}
