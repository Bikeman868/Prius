using System;
using Npgsql;
using Prius.Contracts.Interfaces;

namespace Prius.Orm.Results
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
            return new SqlServer.DataReader(_errorReporter).Initialize(reader, dataShapeName, closeAction, errorAction);
        }

        public IDataReader Create(global::MySql.Data.MySqlClient.MySqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction)
        {
            return new MySql.DataReader(_errorReporter).Initialize(reader, dataShapeName, closeAction, errorAction);
        }

        public IDataReader Create(NpgsqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction)
        {
            return new PostgreSql.DataReader(_errorReporter).Initialize(reader, dataShapeName, closeAction, errorAction);
        }
    }
}
