using System.Collections.Generic;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
{
    public interface IAdoQueryRunner
    {
        void ExecuteNonQuery(SQLiteConnection connection, string sql, IList<IParameter> parameters = null);
        SQLiteDataReader ExecuteReader(SQLiteConnection connection, string sql, IList<IParameter> parameters = null);

        void ExecuteNonQuery(AdoExecutionContext context, string sql);
        SQLiteDataReader ExecuteReader(AdoExecutionContext context, string sql);
    }
}
