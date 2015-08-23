using System;
using Npgsql;
using Prius.Contracts.Interfaces;

namespace Prius.Orm.Commands
{
    public class DataReaderFactory : IDataReaderFactory
    {
        private readonly IErrorReporter _errorReporter;

        public DataReaderFactory(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
        }

        public IDataReader Create(System.Data.SqlClient.SqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction)
        {
            return new DataReader(_errorReporter).Initialize(reader, dataShapeName, closeAction, errorAction);
        }

        public IDataReader Create(MySql.Data.MySqlClient.MySqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction)
        {
            return new MySqlDataReader(_errorReporter).Initialize(reader, dataShapeName, closeAction, errorAction);
        }

        public IDataReader Create(NpgsqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction)
        {
            return new PostgresDataReader(_errorReporter).Initialize(reader, dataShapeName, closeAction, errorAction);
        }

        public IDataReader Create(Exception exception)
        {
            return new DummyDataReader().Initialize(exception);
        }
    }
}
