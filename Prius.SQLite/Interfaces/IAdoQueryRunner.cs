using System.Collections.Generic;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.SQLite.Procedures;
using Prius.SQLite.QueryBuilder;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// Provides a query execution mechanism using the ADO.Net driver
    /// for SQLite in System.Data.SQLite
    /// </summary>
    public interface IAdoQueryRunner
    {
        int ExecuteNonQuery(SQLiteConnection connection, string sql, IList<IParameter> parameters = null);
        SQLiteDataReader ExecuteReader(SQLiteConnection connection, string sql, IList<IParameter> parameters = null);
        T ExecuteScaler<T>(SQLiteConnection connection, string sql, IList<IParameter> parameters = null);

        int ExecuteNonQuery(AdoExecutionContext context, string sql);
        SQLiteDataReader ExecuteReader(AdoExecutionContext context, string sql);
        T ExecuteScaler<T>(AdoExecutionContext context, string sql);

        int ExecuteNonQuery(AdoExecutionContext context, IQuery sql);
        SQLiteDataReader ExecuteReader(AdoExecutionContext context, IQuery sql);
        T ExecuteScaler<T>(AdoExecutionContext context, IQuery sql);
    }
}
