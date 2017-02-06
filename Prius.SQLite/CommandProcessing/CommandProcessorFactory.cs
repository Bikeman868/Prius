using System;
using System.Data.SQLite;
using Prius.Contracts.Enumerations;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    internal class CommandProcessorFactory : ICommandProcessorFactory
    {
        private readonly IProcedureLibrary _storedProcedureLibrary;
        private readonly IProcedureRunner _storedProcedureRunner;
        private readonly IDataReaderFactory _dataReaderFactory;

        public CommandProcessorFactory(
            IProcedureLibrary storedProcedureLibrary, 
            IProcedureRunner storedProcedureRunner, 
            IDataReaderFactory dataReaderFactory)
        {
            _storedProcedureLibrary = storedProcedureLibrary;
            _storedProcedureRunner = storedProcedureRunner;
            _dataReaderFactory = dataReaderFactory;
        }

        public ICommandProcessor Create(
            IRepository repository,
            ICommand command, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            ICommandProcessor commandProcessor = null;

            switch (command.CommandType)
            {
                case CommandType.SQL:
                    commandProcessor = new SqlCommandProcessor(
                        _dataReaderFactory);
                    break;
                case CommandType.StoredProcedure:
                    commandProcessor = new StoredProcedureCommandProcessor(
                        _storedProcedureLibrary,
                        _storedProcedureRunner);
                    break;
            }

            if (commandProcessor == null)
                throw new Exception("The SQLite Prius driver does not support commands of type " + command.CommandType);

            commandProcessor.Initialize(repository, command, connection, transaction);
            return commandProcessor;
        }
    }
}
