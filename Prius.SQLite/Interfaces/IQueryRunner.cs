using System.Data.SQLite;
using System.Text;

namespace Prius.SqLite.Interfaces
{
    public interface IQueryRunner
    {
        void ExecuteNonQuery(SQLiteConnection connection, StringBuilder sql);
        SQLiteDataReader ExecuteReader(SQLiteConnection connection, StringBuilder sql);
    }
}
