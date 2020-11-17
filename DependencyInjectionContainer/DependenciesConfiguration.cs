using System;
using System.Collections.Generic;
using System.Linq;
using DependencyInjectionContainer.Extensions;

namespace DependencyInjectionContainer
{
    public class DependenciesConfiguration : IDependenciesConfiguration
    {
        protected readonly Dictionary<Type, List<ImplementationContainer>> implementations;

        public void Register<TDependency, TImplementation>(bool isSingleton = false, string name = null)
            where TDependency : class
            where TImplementation : TDependency
        {
            Register(typeof(TDependency), typeof(TImplementation), isSingleton, name);
        }



        public IEnumerable<ImplementationContainer> GetImplementations(Type type)
        {
            Type collectionType;

            if (type.IsGenericType)
            {
                collectionType = type.GetGenericTypeDefinition();
            }
            else
            {
                collectionType = type;
            }

            if (implementations.TryGetValue(collectionType, 
                out List<ImplementationContainer> dependencyImplementations))
            {
                IEnumerable<ImplementationContainer> result = 
                    new List<ImplementationContainer>(dependencyImplementations);
                if (type.IsGenericType)
                {
                    result = result.Where((impl) => impl.ImplementationType.IsGenericTypeDefinition 
                                                    || type.IsAssignableFrom(impl.ImplementationType));
                }

                return result;
            }
            else
            {
                return new List<ImplementationContainer>();
            }
        }

        public DependenciesConfiguration()
        {
            implementations = new Dictionary<Type, List<ImplementationContainer>>();
        }
    }
}
