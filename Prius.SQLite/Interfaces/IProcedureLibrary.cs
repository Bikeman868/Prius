using System.Data.SQLite;

namespace Prius.SqLite.Interfaces
{
    public interface IProcedureLibrary
    {
        IProcedure Get(SQLiteConnection connection, string procedureName);
        void Reuse(IProcedure procedure);
    }
}
