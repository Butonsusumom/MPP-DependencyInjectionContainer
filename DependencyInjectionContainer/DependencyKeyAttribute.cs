using System;

namespace DependencyInjectionContainer
{
    public class DependencyKeyAttribute : Attribute
    {
        public string Name
        { get; protected set; }

        public DependencyKeyAttribute(string name)
        {
            Name = name;
        }
    }
}
