using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Enumerations;
using Prius.Contracts.Interfaces.External;

namespace Prius.SqLite
{
    internal class CommandProcessorFactory : ICommandProcessorFactory
    {
        private readonly IErrorReporter _errorReporter;

        public CommandProcessorFactory(
            IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
        }

        public ICommandProcessor Create(
            ICommand command, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            switch (command.CommandType)
            {
                case CommandType.SQL:
                    return new SqlCommandProcessor(_errorReporter, command, connection, transaction);
                case CommandType.StoredProcedure:
                    return new StoredProcedureCommandProcessor(_errorReporter, command, connection, transaction);
            }
            throw new Exception("The SQLite Prius driver does not support commands of type " + command.CommandType);
        }
    }
}
