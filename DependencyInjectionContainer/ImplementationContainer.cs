using System;

namespace DependencyInjectionContainer
{
    public class ImplementationContainer
    {
        public Type ImplementationType
        { get; }

        public bool IsSingleton
        { get; }

        public object SingletonInstance
        { get; set; }

        public string Name
        { get; }

        public ImplementationContainer(Type implementationType, bool isSingleton, string name)
        {
            ImplementationType = implementationType;
            IsSingleton = isSingleton;
            Name = name;
            SingletonInstance = null;
        }
    }
}
