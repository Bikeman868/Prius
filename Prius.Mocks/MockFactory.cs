using System;
using Moq.Modules;
using Prius.Contracts.Interfaces.External;

namespace Prius.Mocks
{
    public class MockFactory: ConcreteImplementationProvider<IFactory>, IFactory
    {
        private IMockProducer _mockProducer;

        protected override IFactory GetImplementation(IMockProducer mockProducer)
        {
            _mockProducer = mockProducer;
            return this;
        }

        public T Create<T>() where T : class
        {
            var type = typeof (T);

            if (type.IsClass && !type.IsAbstract)
            {
                var o = Create(type);
                if (o != null) return (T)o;
            }

            return _mockProducer.SetupMock<T>();
        }

        public object Create(Type type)
        {
            var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor != null)
                return defaultConstructor.Invoke(new object[0]);
            return null;
        }
    }
}
