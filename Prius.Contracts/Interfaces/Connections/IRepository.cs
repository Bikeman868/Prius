using System;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.Connections
{
    public interface IRepository: IDisposable
    {
        string Name { get; }
        IConnection GetConnection(ICommand command);
        void RecordSuccess(IConnection connection, double elapsedSeconds);
        void RecordFailure(IConnection connection);
    }
}
