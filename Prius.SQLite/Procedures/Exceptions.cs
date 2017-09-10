using Prius.Contracts.Exceptions;
using Prius.Contracts.Interfaces.Commands;
using System;
using System.Data.SQLite;

namespace Prius.SQLite.Procedures
{
    public class FailedAdoProcedureException : PriusException
    {
        public ICommand Command { get; private set; }
        public SQLiteConnection Connection { get; private set; }

        public FailedAdoProcedureException(
            string message,
            Exception exception,
            ICommand command,
            SQLiteConnection connection)
            : base(message, exception)
        {
            Command = command;
            Connection = connection;
        }
    }

    public class AdoProcedureTimeoutException: FailedAdoProcedureException
    {
        public int TimeoutSeconds { get; private set; }
        public string Operation { get; private set; }

        public AdoProcedureTimeoutException(
            ICommand command,
            SQLiteConnection connection,
            int timeoutSeconds,
            string operation)
            : base("Procedure '" + command.CommandText + "' did not complete after " + timeoutSeconds + "s", null, command, connection)
        {
            TimeoutSeconds = timeoutSeconds;
            Operation = operation;
        }
    }

    public class AdoProcedureException : FailedAdoProcedureException
    {
        public string Operation { get; private set; }

        public AdoProcedureException(
            ICommand command,
            SQLiteConnection connection,
            Exception exception,
            string operation)
            : base("Procedure '" + command.CommandText + "' threw exception '" + exception.Message + "' during " + operation, exception, command, connection)
        {
            Operation = operation;
        }
    }
}
