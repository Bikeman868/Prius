using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    public interface ICommandProcessor: IDisposable
    {
        ICommandProcessor Initialize(
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction);

        int CommandTimeout { get; set; }

        SQLiteParameterCollection Parameters { get; }

        IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction);
        long ExecuteNonQuery();
        T ExecuteScalar<T>();
    }
}
