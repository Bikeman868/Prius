using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    public class AdoQueryRunner: IAdoQueryRunner
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
                command.CommandText = sql;
                return command.ExecuteReader();
            }
        }

        public void ExecuteNonQuery(Procedures.AdoExecutionContext context, string sql)
        {
            ExecuteNonQuery(context.Connection, sql, context.Parameters);
        }

        public SQLiteDataReader ExecuteReader(Procedures.AdoExecutionContext context, string sql)
        {
            return ExecuteReader(context.Connection, sql, context.Parameters);
        }
    }
}
