using System.Data;

namespace Prius.SqLite.Interfaces
{
    public interface IColumnTypeMapper
    {
        string MapToSqLite(DbType type);
    }
}
