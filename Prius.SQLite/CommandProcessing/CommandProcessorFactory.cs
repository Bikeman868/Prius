using System;
using System.Data.SQLite;
using Prius.Contracts.Enumerations;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    internal class CommandProcessorFactory : ICommandProcessorFactory
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IStoredProcedureLibrary _storedProcedureLibrary;
        private readonly IStoredProcedureRunner _storedProcedureRunner;

        public CommandProcessorFactory(
            IErrorReporter errorReporter, 
            IStoredProcedureLibrary storedProcedureLibrary, 
            IStoredProcedureRunner storedProcedureRunner)
        {
            _errorReporter = errorReporter;
            _storedProcedureLibrary = storedProcedureLibrary;
            _storedProcedureRunner = storedProcedureRunner;
        }

        public ICommandProcessor Create(
            ICommand command, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            ICommandProcessor commandProcessor = null;

            switch (command.CommandType)
            {
                case CommandType.SQL:
                    commandProcessor = new SqlCommandProcessor(_errorReporter);
                    break;
                case CommandType.StoredProcedure:
                    commandProcessor = new StoredProcedureCommandProcessor(
                        _storedProcedureLibrary,
                        _storedProcedureRunner);
                    break;
            }

            if (commandProcessor == null)
                throw new Exception("The SQLite Prius driver does not support commands of type " + command.CommandType);

            commandProcessor.Initialize(command, connection, transaction);
            return commandProcessor;
        }
    }
}
