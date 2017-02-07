using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Utility;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    internal class StoredProcedureCommandProcessor: Disposable, ICommandProcessor
    {
        private readonly IProcedureLibrary _procedureLibrary;
        private readonly IProcedureRunner _procedureRunner;

        private ICommand _command;
        private SQLiteConnection _connection;
        private SQLiteTransaction _transaction;
        private IProcedure _storedProcedure;

        public StoredProcedureCommandProcessor(
            IProcedureLibrary procedureLibrary, 
            IProcedureRunner procedureRunner)
        {
            _procedureLibrary = procedureLibrary;
            _procedureRunner = procedureRunner;
        }

        public ICommandProcessor Initialize (
            IRepository repository,
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            _command = command;
            _connection = connection;
            _transaction = transaction;

            if (command.TimeoutSeconds.HasValue)
                CommandTimeout = command.TimeoutSeconds.Value;

            _storedProcedure = _procedureLibrary.Get(connection, repository.Name, command.CommandText);

            if (_storedProcedure == null)
                throw new Exception("There is no stored procedure with the name " + command.CommandText);

            return this;
        }

        public int CommandTimeout { get; set; }

        public void AddParameter(IParameter parameter)
        {
        }

        public void AddParameters(IEnumerable<IParameter> parameters)
        {
            foreach (var parameter in parameters)
                AddParameter(parameter);
        }

        public IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction)
        {
            return _procedureRunner.ExecuteReader(
                _storedProcedure, 
                _command, 
                CommandTimeout, 
                _connection, 
                _transaction, 
                dataShapeName, 
                closeAction, 
                errorAction);
        }

        public long ExecuteNonQuery()
        {
            return _procedureRunner.ExecuteNonQuery(
                _storedProcedure, 
                _command, 
                CommandTimeout, 
                _connection, 
                _transaction);
        }

        public T ExecuteScalar<T>()
        {
            return _procedureRunner.ExecuteScalar<T>(
                _storedProcedure, 
                _command, 
                CommandTimeout, 
                _connection, 
                _transaction);
        }
    }
}
