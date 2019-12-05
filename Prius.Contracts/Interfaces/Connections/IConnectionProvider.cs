using System;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Connections
{
    public interface IConnectionProvider
    {
        IConnection Open(
            IRepository repository, 
            ICommand command, 
            string connectionString, 
            string schema,
            ITraceWriter traceWriter = null,
            IAnalyticRecorder analyticRecorder = null);
    }
}
