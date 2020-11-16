namespace DependencyInjectionContainer.UnitTests.AccessoryClasses
{
    class MyGenericImplementation1<T> : IMyGenericInterface<T>
        where T: IMyInterface
    {
        public T field;

        public MyGenericImplementation1(T dep)
        {
            field = dep;
        }
    }
}
