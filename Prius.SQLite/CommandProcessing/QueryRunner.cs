using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    public class QueryRunner: IQueryRunner
    {
        public void ExecuteNonQuery(SQLiteConnection connection, string sql, IList<IParameter> parameters)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                command.ExecuteNonQuery();
            }
        }

        public SQLiteDataReader ExecuteReader(SQLiteConnection connection, string sql, IList<IParameter> parameters)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql.ToString();
                return command.ExecuteReader();
            }
        }
    }
}
