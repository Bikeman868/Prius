using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.External;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;

namespace Prius.SqLite.CommandProcessing
{
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
            return new DataReader(_errorReporter)
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
            throw new NotImplementedException();
        }

    }
}
