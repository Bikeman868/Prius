using System;
using Prius.Contracts.Interfaces;

namespace Prius.Orm.Connections
{
    public class ConnectionFactory: IConnectionFactory
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;
        private readonly IDataReaderFactory _dataReaderFactory;

        public ConnectionFactory(
            IErrorReporter errorReporter,
            IDataEnumeratorFactory dataEnumeratorFactory, 
            IDataReaderFactory dataReaderFactory)
        {
            _errorReporter = errorReporter;
            _dataEnumeratorFactory = dataEnumeratorFactory;
            _dataReaderFactory = dataReaderFactory;
        }

        public IConnection CreateSqlServer(IRepository repository, ICommand command, string connectionString)
        {
            return new SqlServerConnection(_errorReporter, _dataEnumeratorFactory, _dataReaderFactory)
                .Initialize(repository, command, connectionString);
        }

        public IConnection CreateMySql(IRepository repository, ICommand command, string connectionString)
        {
            return new MySqlServerConnection(_errorReporter, _dataEnumeratorFactory, _dataReaderFactory)
                .Initialize(repository, command, connectionString);
        }

        public IConnection CreatePostgreSql(IRepository repository, ICommand command, string connectionString, string schema)
        {
            return new PostgreSqlConnection(_errorReporter, _dataEnumeratorFactory, _dataReaderFactory)
                .Initialize(repository, command, connectionString, schema);
        }

        public IConnection CreateRedis(IRepository repository, ICommand command, string hostName, string repositoryName)
        {
            throw new NotImplementedException("Redis connections are not currently supported");
        }
    }
}
