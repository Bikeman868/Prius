namespace Prius.Contracts.Interfaces
{
    public interface IConnectionFactory
    {
        IConnection CreateSqlServer(IRepository repository, ICommand command, string connectionString);
        IConnection CreatePostgreSql(IRepository repository, ICommand command, string connectionString, string schema);
        IConnection CreateRedis(IRepository repository, ICommand command, string hostName, string repositoryName);
        IConnection CreateMySql(IRepository repository, ICommand command, string connectionString);
    }
}
