using System;
using System.Data.SQLite;
using Prius.Contracts.Enumerations;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    /// <summary>
    /// This class examines the database comand and builds a command
    /// processor that knows how to handle this type of command. In
    /// this version SQL statements are handled by SqlCommandProcessor
    /// and stored procedure execution is handled by ProcedureCommandProcessor
    /// </summary>
    internal class CommandProcessorFactory : ICommandProcessorFactory
    {
        private readonly IProcedureLibrary _storedProcedureLibrary;
        private readonly IProcedureRunner _storedProcedureRunner;
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IParameterConverter _parameterConverter;

        public CommandProcessorFactory(
            IProcedureLibrary storedProcedureLibrary, 
            IProcedureRunner storedProcedureRunner, 
            IDataReaderFactory dataReaderFactory, 
            IParameterConverter parameterConverter)
        {
            _storedProcedureLibrary = storedProcedureLibrary;
            _storedProcedureRunner = storedProcedureRunner;
            _dataReaderFactory = dataReaderFactory;
            _parameterConverter = parameterConverter;
        }

        public ICommandProcessor CreateAdo(
            IRepository repository,
            ICommand command, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            IAdoCommandProcessor commandProcessor = null;

            switch (command.CommandType)
            {
                case CommandType.SQL:
                    commandProcessor = new SqlCommandProcessor(
                        _dataReaderFactory,
                        _parameterConverter);
                    break;
                case CommandType.StoredProcedure:
                    commandProcessor = new AdoProcedureCommandProcessor(
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
