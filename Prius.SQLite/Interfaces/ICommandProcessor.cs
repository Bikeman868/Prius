using System;
using Prius.Contracts.Interfaces;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// This is the base interface for all classes that know how 
    /// to process database commands. There are various implementations
    /// that execute specific kinds of command or using a specific method
    /// (for example natively accessing the sqlite engine or going via
    /// the ADO.Net driver for SQLite).
    /// </summary>
    public interface ICommandProcessor: IDisposable
    {
        int CommandTimeout { get; set; }

        IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction);
        long ExecuteNonQuery();
        T ExecuteScalar<T>();
    }
}
