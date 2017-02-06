using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Utility;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    internal class StoredProcedureCommandProcessor: Disposable, ICommandProcessor
    {
        private readonly IStoredProcedureLibrary _storedProcedureLibrary;
        private readonly IStoredProcedureRunner _storedProcedureRunner;

        private ICommand _command;
        private SQLiteConnection _connection;
        private SQLiteTransaction _transaction;
        private IStoredProcedure _storedProcedure;

        public StoredProcedureCommandProcessor(
            IStoredProcedureLibrary storedProcedureLibrary, 
            IStoredProcedureRunner storedProcedureRunner)
        {
            _storedProcedureLibrary = storedProcedureLibrary;
            _storedProcedureRunner = storedProcedureRunner;
        }

        public ICommandProcessor Initialize (
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            _command = command;
            _connection = connection;
            _transaction = transaction;

            _storedProcedure = _storedProcedureLibrary.GetProcedure(connection, command.CommandText);
            if (_storedProcedure == null)
                throw new Exception("There is no stored procedure with the name " + command.CommandText);

            return this;
        }

        public int CommandTimeout { get; set; }

        public SQLiteParameterCollection Parameters
        {
            get { throw new NotImplementedException(); }
        }

        public IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction)
        {
            return _storedProcedureRunner.ExecuteReader(_storedProcedure, _command, CommandTimeout, _connection, _transaction);
        }

        public long ExecuteNonQuery()
        {
            return _storedProcedureRunner.ExecuteNonQuery(_storedProcedure, _command, CommandTimeout, _connection, _transaction);
        }

        public object ExecuteScalar()
        {
            return _storedProcedureRunner.ExecuteScalar(_storedProcedure, _command, CommandTimeout, _connection, _transaction);
        }
    }
}
