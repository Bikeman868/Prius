using Microsoft.Practices.Unity;
using Prius.Contracts.Interfaces.External;

namespace Prius.Performance.Prius.Integration
{
    public class Factory: IFactory
    {
        private readonly UnityContainer _container;

        public Factory(UnityContainer container)
        {
            _container = container;
        }

        public T Create<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public object Create(System.Type type)
        {
            return _container.Resolve(type);
        }
    }
}
