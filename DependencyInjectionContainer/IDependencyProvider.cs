using System.Collections.Generic;

namespace DependencyInjectionContainer
{
    public interface IDependencyProvider
    {
        IEnumerable<TDependency> Resolve<TDependency>(string name = null)
            where TDependency : class;
    }
}
