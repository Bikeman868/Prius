using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using Prius.Contracts.Interfaces;
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
                var defaultConstructor = type.GetConstructor(Type.EmptyTypes);
                if (defaultConstructor != null)
                    return (T)defaultConstructor.Invoke(new object[0]);
            }

            return _mockProducer.SetupMock<T>();
        }
    }
}
