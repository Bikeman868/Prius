using System;

namespace Prius.Contracts.Interfaces
{
    public interface IRepository: IDisposable
    {
        string Name { get; }
        IConnection GetConnection(ICommand command);
        void RecordSuccess(IConnection connection, double elapsedSeconds);
        void RecordFailure(IConnection connection);
    }
}
