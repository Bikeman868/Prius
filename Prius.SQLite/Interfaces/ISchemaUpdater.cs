using System.Data.SQLite;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// This interface can be implemented by the application developer to 
    /// check and adjust their SQLite database schema before it is accessed
    /// for the first time.
    /// </summary>
    public interface ISchemaUpdater
    {
        void CheckSchema(IRepository repository, SQLiteConnection connection);
    }
}
