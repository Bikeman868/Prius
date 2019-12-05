using System;
using Moq;
using Moq.Modules;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Mocks.Helper;

namespace Prius.Mocks
{
    public class MockConnectionFactory: MockImplementationProvider<IConnectionFactory>
    {
        public IMockedRepository MockedRepository { get; set; }

        protected override void SetupMock(IMockProducer mockProducer, Mock<IConnectionFactory> mock)
        {
            mock.Setup(cf => cf.Create(
                    It.IsAny<string>(), 
                    It.IsAny<IRepository>(), 
                    It.IsAny<ICommand>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<ITraceWriter>(),
                    It.IsAny<IAnalyticRecorder>()))
                .Returns((
                    string serverType, 
                    IRepository r, 
                    ICommand cmd, 
                    string cs, 
                    string schema,
                    ITraceWriter traceWriter,
                    IAnalyticRecorder analyticRecorder) => new Connection(MockedRepository, cmd));
        }

    }
}
