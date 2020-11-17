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

        public void Register(Type dependency, Type implementation, bool isSingleton = false, string name = null)
        {
            if (dependency.IsGenericTypeDefinition ^ implementation.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Open generics register should be with both open generic types");
            }

            if (dependency.IsGenericTypeDefinition)
            {
                if (isSingleton)
                {
                    throw new ArgumentException("Open generic cannot be singleton");
                }

                if (!dependency.IsAssignableFromAsOpenGeneric(implementation))
                {
                    throw new ArgumentException("Dependency is not assignable from implementation");
                }
            }
            else
            {
                if (!dependency.IsClass && !dependency.IsAbstract && !dependency.IsInterface 
                    || (!implementation.IsClass || implementation.IsAbstract))
                {
                    throw new ArgumentException("Wrong types");
                }

                if (!dependency.IsAssignableFrom(implementation))
                {
                    throw new ArgumentException("Dependency is not assignable from implementation");
                }
            }

            ImplementationContainer container = new ImplementationContainer(implementation, isSingleton, name);
            if (dependency.IsGenericType)
            {
                dependency = dependency.GetGenericTypeDefinition();
            }

            List<ImplementationContainer> dependencyImplementations;
            lock (implementations)
            {
                if (!implementations.TryGetValue(dependency, out dependencyImplementations))
                {
                    dependencyImplementations = new List<ImplementationContainer>();
                    implementations[dependency] = dependencyImplementations;
                }
            }

            lock (dependencyImplementations)
            {
                if (name != null)
                {
                    dependencyImplementations.RemoveAll((existingContainer) => existingContainer.Name == name);
                }
                dependencyImplementations.Add(container);
            }
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
