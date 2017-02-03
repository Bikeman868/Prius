using System;
using Moq;
using Moq.Modules;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.Factory;
using Prius.Mocks.Helper;

namespace Prius.Mocks
{
    public class MockConnectionFactory: MockImplementationProvider<IConnectionFactory>
    {
        public IMockedRepository MockedRepository { get; set; }

        protected override void SetupMock(IMockProducer mockProducer, Mock<IConnectionFactory> mock)
        {
            mock.Setup(cf => cf.Create(It.IsAny<string>(), It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string serverType, IRepository r, ICommand cmd, string cs, string schema) => new Connection(MockedRepository, cmd));
        }

    }
}
