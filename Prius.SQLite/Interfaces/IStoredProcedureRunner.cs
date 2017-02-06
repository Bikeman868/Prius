using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    public interface IStoredProcedureRunner
    {
        IDataReader ExecuteReader(IStoredProcedure storedProcedure, ICommand command, int commandTimeout, SQLiteConnection connection, SQLiteTransaction transaction);
        object ExecuteScalar(IStoredProcedure storedProcedure, ICommand command, int commandTimeout, SQLiteConnection connection, SQLiteTransaction transaction);
        long ExecuteNonQuery(IStoredProcedure storedProcedure, ICommand command, int commandTimeout, SQLiteConnection connection, SQLiteTransaction transaction);
    }
}
