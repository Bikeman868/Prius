using System;
using Moq;
using Moq.Modules;
using Prius.Contracts.Interfaces;
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
            mock.Setup(cf => cf.CreateMySql(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string c) => new Connection(MockedRepository, cmd));

            mock.Setup(cf => cf.CreatePostgreSql(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string c, string schema) => new Connection(MockedRepository, cmd));

            mock.Setup(cf => cf.CreateRedis(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string host, string repo) => new Connection(MockedRepository, cmd));

            mock.Setup(cf => cf.CreateSqlServer(It.IsAny<IRepository>(), It.IsAny<ICommand>(), It.IsAny<string>()))
                .Returns((IRepository r, ICommand cmd, string c) => new Connection(MockedRepository, cmd));
        }

    }
}
