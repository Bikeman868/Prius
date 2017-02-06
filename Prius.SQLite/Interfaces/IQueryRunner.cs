using System.Collections.Generic;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    public interface IQueryRunner
    {
        void ExecuteNonQuery(SQLiteConnection connection, string sql, IList<IParameter> parameters = null);
        SQLiteDataReader ExecuteReader(SQLiteConnection connection, string sql, IList<IParameter> parameters = null);
    }
}
