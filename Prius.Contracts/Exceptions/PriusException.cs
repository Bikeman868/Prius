using System;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Exceptions
{
    public class PriusException: ApplicationException
    {
        public ICommand Command { get; private set; }
        public IConnection Connection { get; private set; }
        public IRepository Repository { get; private set; }

        public PriusException()
        { }

        public PriusException(string message
            ): base(message)
        { }

        public PriusException(string message, Exception innerException) 
            : base(message, innerException)
        { }

        public PriusException(
            string message, 
            Exception innerException, 
            ICommand command, 
            IConnection connection, 
            IRepository repository)
            : base(message, innerException)
        {
            Command = command;
            Connection = connection;
            Repository = repository;
        }
    }
}
