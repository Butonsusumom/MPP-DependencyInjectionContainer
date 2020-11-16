using Microsoft.VisualStudio.TestTools.UnitTesting;
using DependencyInjectionContainer.UnitTests.AccessoryClasses;
using System.Linq;
using System;
using System.Collections.Generic;

namespace DependencyInjectionContainer.UnitTests
{
    [TestClass]
    public class DependencyInjectionContainerTests
    {
        IDependenciesConfiguration config;
        IDependencyProvider provider;

        [TestInitialize]
        public void TestInitialize()
        {
            config = new DependenciesConfiguration();
        }

        [TestMethod]
        public void NonGenericTypeRegisterTest()
        {
            config.Register<IMyInterface, MyImplementation1>();
            config.Register<IMyInterface, MyImplementation2>();
            var registeredImplementations = config.GetImplementations(typeof(IMyInterface)).ToList();
            Assert.AreEqual(2, registeredImplementations.Count);

            List<Type> expectedRegisteredTypes = new List<Type>
            {
                typeof(MyImplementation1),
                typeof(MyImplementation2)
            };
            CollectionAssert.AreEquivalent(expectedRegisteredTypes, 
                registeredImplementations.Select((implementation) => implementation.ImplementationType).ToList());
        }

        [TestMethod]
        public void GenericTypeRegisterTest()
        {
            config.Register<IMyGenericInterface<IMyInterface>, MyGenericImplementation1<IMyInterface>>();
            config.Register<IMyGenericInterface<IMyInterface>, MyGenericImplementation2<IMyInterface>>();
            var registeredImplementations = config.GetImplementations(typeof(IMyGenericInterface<IMyInterface>))
                .ToList();
            Assert.AreEqual(2, registeredImplementations.Count);

            List<Type> expectedRegisteredTypes = new List<Type>
            {
                typeof(MyGenericImplementation1<IMyInterface>),
                typeof(MyGenericImplementation2<IMyInterface>)
            };
            CollectionAssert.AreEquivalent(expectedRegisteredTypes,
                registeredImplementations.Select((implementation) => implementation.ImplementationType).ToList());
        }

        [TestMethod]
        public void OpenGenericTypeRegisterTest()
        {
            config.Register(typeof(IMyGenericInterface<>), typeof(MyGenericImplementation1<>));
            config.Register(typeof(IMyGenericInterface<>), typeof(MyGenericImplementation2<>));
            var registeredImplementations = config.GetImplementations(typeof(IMyGenericInterface<>)).ToList();
            Assert.AreEqual(2, registeredImplementations.Count);

            List<Type> expectedRegisteredTypes = new List<Type>
            {
                typeof(MyGenericImplementation1<>),
                typeof(MyGenericImplementation2<>)
            };
            CollectionAssert.AreEquivalent(expectedRegisteredTypes,
                registeredImplementations.Select((implementation) => implementation.ImplementationType).ToList());
        }

        [TestMethod]
        public void NonGenericTypeResolveTest()
        {
            config.Register<IMyInterface, MyImplementation1>();
            config.Register<IMyInterface, MyImplementation2>();
            provider = new DependencyProvider(config);
            var instances = provider.Resolve<IMyInterface>();
            Assert.AreEqual(2, instances.Count());

            List<Type> expectedInstancesTypes = new List<Type>
            {
                typeof(MyImplementation1),
                typeof(MyImplementation2)
            };
            CollectionAssert.AreEquivalent(expectedInstancesTypes,
                instances.Select((instance) => instance.GetType()).ToList());
        }

        [TestMethod]
        public void GenericTypeResolveTest()
        {
            config.Register<IMyGenericInterface<IMyInterface>, MyGenericImplementation1<IMyInterface>>();
            config.Register<IMyGenericInterface<IMyInterface>, MyGenericImplementation2<IMyInterface>>();
            provider = new DependencyProvider(config);
            var instances = provider.Resolve<IMyGenericInterface<IMyInterface>>();
            Assert.AreEqual(2, instances.Count());

            List<Type> expectedInstancesTypes = new List<Type>
            {
                typeof(MyGenericImplementation1<IMyInterface>),
                typeof(MyGenericImplementation2<IMyInterface>)
            };
            CollectionAssert.AreEquivalent(expectedInstancesTypes,
                instances.Select((instance) => instance.GetType()).ToList());
        }

        [TestMethod]
        public void OpenGenericTypeResolveTest()
        {
            config.Register<IMyInterface, MyImplementation1>();
            config.Register(typeof(IMyGenericInterface<>), typeof(MyGenericImplementation1<>));
            config.Register(typeof(IMyGenericInterface<>), typeof(MyGenericImplementation2<>));
            provider = new DependencyProvider(config);
            var instances = provider.Resolve<IMyGenericInterface<IMyInterface>>();
            Assert.AreEqual(2, instances.Count());

            List<Type> expectedInstancesTypes = new List<Type>
            {
                typeof(MyGenericImplementation1<IMyInterface>),
                typeof(MyGenericImplementation2<IMyInterface>)
            };
            CollectionAssert.AreEquivalent(expectedInstancesTypes,
                instances.Select((instance) => instance.GetType()).ToList());

            Assert.AreEqual(typeof(MyImplementation1), 
                instances.OfType<MyGenericImplementation1<IMyInterface>>().First().field.GetType());
        }

        [TestMethod]
        public void SingletonResolveTest()
        {
            config.Register<IMyInterface, MyImplementation1>(true);
            provider = new DependencyProvider(config);

            Assert.ReferenceEquals(provider.Resolve<IMyInterface>().First(), provider.Resolve<IMyInterface>().First());
        }

        [TestMethod]
        public void ExplicitNameResolveTest()
        {
            config.Register<IMyInterface, MyImplementation1>(name: "1");
            config.Register<IMyInterface, MyImplementation2>(name: "2");
            provider = new DependencyProvider(config);
            IEnumerable<IMyInterface> instances;

            instances = provider.Resolve<IMyInterface>("1");
            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(typeof(MyImplementation1), instances.First().GetType());

            instances = provider.Resolve<IMyInterface>("2");
            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(typeof(MyImplementation2), instances.First().GetType());
        }

        [TestMethod]
        public void ConstructorNameResolveTest()
        {
            config.Register<IMyInterface, MyImplementation1>(name: "1");
            config.Register<IMyInterface, MyImplementation2>(name: "2");
            config.Register<IMyInterface, MyNamedConstructorParameterImplementation>();
            provider = new DependencyProvider(config);
            var instances = provider.Resolve<IMyInterface>().OfType<MyNamedConstructorParameterImplementation>();

            Assert.AreEqual(1, instances.Count());
            Assert.AreEqual(typeof(MyImplementation1), instances.First().intfImpl1.GetType());
            Assert.AreEqual(typeof(MyImplementation2), instances.First().intfImpl2.GetType());
        }
    }
}
