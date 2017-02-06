using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
{
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
