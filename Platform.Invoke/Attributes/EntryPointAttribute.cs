using System;

namespace Platform.Invoke.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EntryPointAttribute : Attribute
    {
        public string Name;

        public EntryPointAttribute(string name)
        {
            this.Name = name;
        }
    }
}
