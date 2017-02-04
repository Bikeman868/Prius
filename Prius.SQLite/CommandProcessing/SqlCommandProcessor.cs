using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    internal class SqlCommandProcessor: ICommandProcessor
    {
        private readonly IErrorReporter _errorReporter;
        private readonly SQLiteCommand _command;

        public SqlCommandProcessor(
            IErrorReporter errorReporter,
            ICommand command, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            _errorReporter = errorReporter;
            _command = new SQLiteCommand(command.CommandText, connection, transaction);
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
