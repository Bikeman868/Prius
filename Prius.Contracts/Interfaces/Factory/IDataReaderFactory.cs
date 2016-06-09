using System;
using Npgsql;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IDataReaderFactory
    {
        IDataReader Create(System.Data.SqlClient.SqlDataReader reader, string dataShapeName, Action closeAction = null, Action errorAction = null);
        IDataReader Create(MySql.Data.MySqlClient.MySqlDataReader reader, string dataShapeName, Action closeAction = null, Action errorAction = null);
        IDataReader Create(NpgsqlDataReader reader, string dataShapeName, Action closeAction = null, Action errorAction = null);
    }
}
