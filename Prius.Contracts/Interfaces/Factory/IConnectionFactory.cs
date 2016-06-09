using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IConnectionFactory
    {
        IConnection CreateSqlServer(IRepository repository, ICommand command, string connectionString);
        IConnection CreatePostgreSql(IRepository repository, ICommand command, string connectionString, string schema);
        IConnection CreateRedis(IRepository repository, ICommand command, string hostName, string repositoryName);
        IConnection CreateMySql(IRepository repository, ICommand command, string connectionString);
    }
}
