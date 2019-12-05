using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IConnectionFactory
    {
        IConnection Create(
            string serverType, 
            IRepository repository, 
            ICommand command, 
            string connectionString, 
            string schema,
            ITraceWriter traceWriter = null,
            IAnalyticRecorder analyticRecorder = null);
    }
}
