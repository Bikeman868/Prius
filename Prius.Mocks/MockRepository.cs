using Moq;
using Moq.Modules;
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
                .Returns((ICommand command) => mockConnectionFactory.Create("SqlServer", mock.Object, command, string.Empty, string.Empty));
        }
    }
}
