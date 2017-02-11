using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.External;
using Prius.SQLite.Interfaces;
using Prius.SQLite.Procedures;

namespace Prius.SQLite.CommandProcessing
{
    /// <summary>
    /// This factory for data readers exists to support dependency
    /// injection. The factory has the same dependencies as the 
    /// data reader by is a singleton so that the dependencies are
    /// only evaluated once at startup.
    /// </summary>
    internal class DataReaderFactory : IDataReaderFactory
    {
        private readonly IErrorReporter _errorReporter;

        public DataReaderFactory(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
        }

        public IDataReader Create(
            SQLiteDataReader sqLiteDataReader,
            string dataShapeName, 
            Action<IDataReader> closeAction, 
            Action<IDataReader> errorAction)
        {
            return new AdoDataReader(_errorReporter)
                .Initialize(sqLiteDataReader, dataShapeName, closeAction, errorAction);
        }

        public IDataReader Create(
            SQLiteDataReader sqLiteDataReader, 
            AdoExecutionContext adoExecutionContext)
        {
            return Create(
                sqLiteDataReader, 
                adoExecutionContext.DataShapeName, 
                adoExecutionContext.CloseAction, 
                adoExecutionContext.ErrorAction);
        }

        public IDataReader Create(
            object resultHandle, 
            NativeExecutionContext nativeExecutionContext)
        {
            // TODO: Add a data reader that supports native access to the SQLite engine
            throw new NotImplementedException();
        }

    }
}
