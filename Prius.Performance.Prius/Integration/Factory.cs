using Microsoft.Practices.Unity;
using Prius.Contracts.Interfaces;

namespace Prius.Performance.Prius
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
    }
}
