using System;
using Npgsql;

namespace Prius.Contracts.Interfaces
{
    public interface IDataReaderFactory
    {
        IDataReader Create(System.Data.SqlClient.SqlDataReader reader, string dataShapeName, Action closeAction = null, Action errorAction = null);
        IDataReader Create(MySql.Data.MySqlClient.MySqlDataReader reader, string dataShapeName, Action closeAction = null, Action errorAction = null);
        IDataReader Create(NpgsqlDataReader reader, string dataShapeName, Action closeAction = null, Action errorAction = null);
    }
}
