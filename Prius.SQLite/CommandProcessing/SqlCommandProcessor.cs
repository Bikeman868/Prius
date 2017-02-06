using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Utility;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    internal class SqlCommandProcessor: Disposable, ICommandProcessor
    {
        private readonly IDataReaderFactory _dataReaderFactory;

        private SQLiteCommand _command;

        public SqlCommandProcessor(
            IDataReaderFactory dataReaderFactory)
        {
            _dataReaderFactory = dataReaderFactory;
        }

        public ICommandProcessor Initialize(
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            _command = new SQLiteCommand(command.CommandText, connection, transaction);
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            _command.Dispose();
            base.Dispose(destructor);
        }

        public int CommandTimeout
        {
            get { return _command.CommandTimeout; }
            set { _command.CommandTimeout = value; }
        }

        public SQLiteParameterCollection Parameters
        {
            get { return _command.Parameters; }
        }

        public IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction)
        {
            var sqLiteDataReader = _command.ExecuteReader();
            return _dataReaderFactory.Create(sqLiteDataReader, dataShapeName, closeAction, errorAction);
        }

        public long ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        public T ExecuteScalar<T>()
        {
            var result = _command.ExecuteScalar();

            if (result == null) return default(T);

            var resultType = typeof(T);
            if (resultType.IsNullable()) resultType = resultType.GetGenericArguments()[0];
            return (T)Convert.ChangeType(result, resultType);
        }
    }
}
