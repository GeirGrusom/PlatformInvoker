using System;

namespace Platform.Invoke.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class LibraryAttribute : Attribute
    {
        public string Name;

        public LibraryAttribute(string name)
        {
            this.Name = name;
        }
    }
}