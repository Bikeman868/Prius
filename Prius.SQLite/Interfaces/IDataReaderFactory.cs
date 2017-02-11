using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.SQLite.Procedures;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// A factory for different kinds of data reader. Can contruct
    /// daatreaders for ADO.Net connections to SQLite and native
    /// connections directly to the SQLite engine.
    /// </summary>
    public interface IDataReaderFactory
    {
        IDataReader Create(
            SQLiteDataReader sqLiteDataReader,
            string dataShapeName,
            Action<IDataReader> closeAction,
            Action<IDataReader> errorAction);

        IDataReader Create(SQLiteDataReader sqLiteDataReader, AdoExecutionContext adoExecutionContext);
        IDataReader Create(object resultHandle, NativeExecutionContext nativeExecutionContext);
    }
}
