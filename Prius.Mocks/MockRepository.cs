using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Moq.Modules;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.Factory;

namespace Prius.Mocks
{
    public class MockRepository: MockImplementationProvider<IRepository>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IRepository> mock)
        {
            var mockConnectionFactory = mockProducer.SetupMock<IConnectionFactory>();

            mock
                .Setup(r => r.GetConnection(It.IsAny<ICommand>()))
                .Returns((ICommand command) => mockConnectionFactory.CreateSqlServer(mock.Object, command, string.Empty));
        }
    }
}
