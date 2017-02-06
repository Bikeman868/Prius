using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Utility;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    internal class SqlCommandProcessor: Disposable, ICommandProcessor
    {
        private readonly IErrorReporter _errorReporter;
        private SQLiteCommand _command;

        public SqlCommandProcessor(
            IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
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
            return new DataReader(_errorReporter).Initialize(sqLiteDataReader, dataShapeName, closeAction, errorAction);
        }

        public long ExecuteNonQuery()
        {
            return _command.ExecuteNonQuery();
        }

        public object ExecuteScalar()
        {
            return _command.ExecuteScalar();
        }
    }
}
