using System.Data;
using System.Data.SQLite;
using System.Text;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    public class QueryRunner: IQueryRunner
    {
        public void ExecuteNonQuery(SQLiteConnection connection, StringBuilder sql)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql.ToString();
                command.ExecuteNonQuery();
            }
        }

        public SQLiteDataReader ExecuteReader(SQLiteConnection connection, StringBuilder sql)
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
