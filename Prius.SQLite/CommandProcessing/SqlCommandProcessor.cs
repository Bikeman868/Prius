using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Utility;
using Prius.SQLite.Interfaces;

namespace Prius.SQLite.CommandProcessing
{
    /// <summary>
    /// This is a command processor for SQL statements. It does not
    /// know how to execute stored procedures, it only supports SQL
    /// statements. There are other command processors for stored procedures.
    /// </summary>
    internal class SqlCommandProcessor: Disposable, IAdoCommandProcessor
    {
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IParameterConverter _parameterConverter;

        private SQLiteCommand _command;

        public SqlCommandProcessor(
            IDataReaderFactory dataReaderFactory, 
            IParameterConverter parameterConverter)
        {
            _dataReaderFactory = dataReaderFactory;
            _parameterConverter = parameterConverter;
        }

        public ICommandProcessor Initialize(
            IRepository repository,
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            _command = new SQLiteCommand(command.CommandText, connection, transaction);

            if (command.TimeoutSeconds.HasValue)
                CommandTimeout = command.TimeoutSeconds.Value;

            foreach (var parameter in command.GetParameters())
                _parameterConverter.AddParameter(_command, parameter);

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
