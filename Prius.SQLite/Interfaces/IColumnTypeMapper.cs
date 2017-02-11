using System.Data;

namespace Prius.SQLite.Interfaces
{
    public interface IColumnTypeMapper
    {
        string MapToSqLite(DbType type);
    }
}
