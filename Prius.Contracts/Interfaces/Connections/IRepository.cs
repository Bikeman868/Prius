using System;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Connections
{
    public interface IRepository: IDisposable
    {
        string Name { get; }
        IConnection GetConnection(ICommand command);
        void RecordSuccess(IConnection connection, double elapsedSeconds);
        void RecordFailure(IConnection connection);
        void EnableTracing(ITraceWriterFactory traceWriterFactory);
    }
}
