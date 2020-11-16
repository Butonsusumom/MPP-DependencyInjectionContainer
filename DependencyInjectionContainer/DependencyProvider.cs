using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace DependencyInjectionContainer
{
    public class DependencyProvider : IDependencyProvider
    {
        protected readonly IDependenciesConfiguration configuration;

        protected readonly ConcurrentDictionary<int, Stack<Type>> recursionTypesExcluder;

        public IEnumerable<TDependency> Resolve<TDependency>(string name = null) 
            where TDependency : class
        {
            Type dependencyType = typeof(TDependency);

            if (dependencyType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Generic type definition resolving is not supproted");
            }
            if (recursionTypesExcluder.TryGetValue(Thread.CurrentThread.ManagedThreadId, out Stack<Type> types))
            {
                types.Clear();
            }
            else
            {
                recursionTypesExcluder[Thread.CurrentThread.ManagedThreadId] = new Stack<Type>();
            }

            return Resolve(dependencyType, name).OfType<TDependency>();
        }

        protected IEnumerable<object> Resolve(Type dependency, string name)
        {
            if (dependency.IsGenericType || dependency.IsGenericTypeDefinition)
            {
                return ResolveGeneric(dependency, name);
            }
            else
            {
                return ResolveNonGeneric(dependency, name);
            }
        }

        protected IEnumerable<object> ResolveGeneric(Type dependency, string name)
        {
            List<object> result = new List<object>();
            IEnumerable<ImplementationContainer> implementationContainers 
                = configuration.GetImplementations(dependency)
                .Where((impl) => !recursionTypesExcluder[Thread.CurrentThread.ManagedThreadId].Contains(impl.ImplementationType));
            if (name != null)
            {
                implementationContainers = implementationContainers.Where((container) => container.Name == name);
            }

            object instance;
            foreach (ImplementationContainer implementationContainer in implementationContainers)
            {
                instance = CreateByConstructor(implementationContainer.ImplementationType.GetGenericTypeDefinition()
                    .MakeGenericType(dependency.GenericTypeArguments));

                if (instance != null)
                {
                    result.Add(instance);
                }
            }

            return result;
        }

        protected IEnumerable<object> ResolveNonGeneric(Type dependency, string name)
        {
            if (dependency.IsValueType)
            {
                return new List<object>
                {
                    Activator.CreateInstance(dependency)
                };
            }

            IEnumerable<ImplementationContainer> implementationContainers = 
                configuration.GetImplementations(dependency)
                .Where((impl) => !recursionTypesExcluder[Thread.CurrentThread.ManagedThreadId].Contains(impl.ImplementationType));
            if (name != null)
            {
                implementationContainers = implementationContainers
                    .Where((implementation) => implementation.Name == name);
            }
            List<object> result = new List<object>();
            object dependencyInstance;

            foreach (ImplementationContainer container in implementationContainers)
            {
                if (container.IsSingleton)
                {
                    if (container.SingletonInstance == null)
                    {
                        lock (container)
                        {
                            if (container.SingletonInstance == null)
                            {
                                container.SingletonInstance = CreateByConstructor(container.ImplementationType);
                            }
                        }
                    }
                    dependencyInstance = container.SingletonInstance;
                }
                else
                {
                    dependencyInstance = CreateByConstructor(container.ImplementationType);
                }

                if (dependencyInstance != null)
                {
                    result.Add(dependencyInstance);
                }
            }
            return result;
        }

        protected object CreateByConstructor(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors()
                .OrderBy((constructor) => constructor.GetParameters().Length).ToArray();
            object instance = null;
            List<object> parameters = new List<object>();
            recursionTypesExcluder[Thread.CurrentThread.ManagedThreadId].Push(type);

            for (int constructor = 0; (constructor < constructors.Length) && (instance == null); ++constructor)
            {
                try
                {
                    foreach (ParameterInfo constructorParameter in constructors[constructor].GetParameters())
                    {
                        parameters.Add(Resolve(constructorParameter.ParameterType, 
                            constructorParameter.GetCustomAttribute<DependencyKeyAttribute>()?.Name).FirstOrDefault());
                    }
                    instance = constructors[constructor].Invoke(parameters.ToArray());
                }
                catch
                {
                    parameters.Clear();
                }
            }

            recursionTypesExcluder[Thread.CurrentThread.ManagedThreadId].Pop();
            return instance;
        }

        public DependencyProvider(IDependenciesConfiguration configuration)
        {
            this.configuration = configuration;
            recursionTypesExcluder = new ConcurrentDictionary<int, Stack<Type>>();
        }
    }
}
