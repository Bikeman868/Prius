using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IConnectionFactory
    {
        IConnection Create(string serverType, IRepository repository, ICommand command, string connectionString, string schema);
    }
}
