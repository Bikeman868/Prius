using System;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.Connections
{
    public interface IConnectionProvider
    {
        IConnection Open(IRepository repository, ICommand command, string connectionString, string schema);
    }
}
