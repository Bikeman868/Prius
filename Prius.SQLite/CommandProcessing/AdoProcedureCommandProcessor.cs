using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Utility;
using Prius.SQLite.Interfaces;

namespace Prius.SQLite.CommandProcessing
{
    /// <summary>
    /// This class executes pseudo stored procedures written as C# classes
    /// using the ADO.Net driver for SQLite in System.Data.SQLite.
    /// </summary>
    internal class AdoProcedureCommandProcessor: Disposable, IAdoCommandProcessor
    {
        private readonly IProcedureLibrary _procedureLibrary;
        private readonly IProcedureRunner _procedureRunner;

        private ICommand _command;
        private SQLiteConnection _connection;
        private SQLiteTransaction _transaction;
        private IProcedure _storedProcedure;

        public AdoProcedureCommandProcessor(
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

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
            {
                _procedureLibrary.Reuse(_storedProcedure);
            }
            base.Dispose(destructor);
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
