namespace DependencyInjectionContainer.UnitTests.AccessoryClasses
{
    class MyNamedConstructorParameterImplementation : IMyInterface
    {
        public readonly IMyInterface intfImpl1;
        public readonly IMyInterface intfImpl2;

        public MyNamedConstructorParameterImplementation([DependencyKey("1")] IMyInterface intfImpl1, 
            [DependencyKey("2")] IMyInterface intfImpl2)
        {
            this.intfImpl1 = intfImpl1;
            this.intfImpl2 = intfImpl2;
        }
    }
}
