using System;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;


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
            return new SqlServer.Connection(_errorReporter, _dataEnumeratorFactory, _dataReaderFactory)
                .Initialize(repository, command, connectionString);
        }

        public IConnection CreateMySql(IRepository repository, ICommand command, string connectionString)
        {
            return new MySql.Connection(_errorReporter, _dataEnumeratorFactory, _dataReaderFactory)
                .Initialize(repository, command, connectionString);
        }

        public IConnection CreatePostgreSql(IRepository repository, ICommand command, string connectionString, string schema)
        {
            return new PostgreSql.Connection(_errorReporter, _dataEnumeratorFactory, _dataReaderFactory)
                .Initialize(repository, command, connectionString, schema);
        }

        public IConnection CreateRedis(IRepository repository, ICommand command, string hostName, string repositoryName)
        {
            throw new NotImplementedException("Redis connections are not currently supported");
        }
    }
}
