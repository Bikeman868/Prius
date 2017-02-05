using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;

namespace Prius.SqLite.Interfaces
{
    public interface ICommandProcessor: IDisposable
    {
        int CommandTimeout { get; set; }
        SQLiteParameterCollection Parameters { get; }

        IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction);
        long ExecuteNonQuery();
        object ExecuteScalar();
    }
}
