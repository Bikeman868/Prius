using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;

namespace UsersTestApp.Integration
{
    internal class AnalyticRecorderFactory: IAnalyticRecorderFactory, IAnalyticRecorder
    {
        public IAnalyticRecorder Create(string repositoryName, ICommand command)
        {
            return this;
        }

        public void ConnectionOpened(ConnectionAnalyticInfo info)
        {
            Console.WriteLine("Connection opened to '" + info.ConnectionString + "'. There are " + info.ActiveCount + " active connections");
        }

        public void ConnectionClosed(ConnectionAnalyticInfo info)
        {
            Console.WriteLine("Connection closed to '" + info.ConnectionString + ". There are " + info.ActiveCount + " active connections");
        }

        public void ConnectionFailed(ConnectionAnalyticInfo info)
        {
            Console.WriteLine("Failed to connect to repository '" + info.RepositoryName + "'");
        }

        public void CommandCompleted(CommandAnalyticInfo info)
        {
            Console.WriteLine("Command '" + info.Command.CommandText + "' completed in " + info.ElapsedSeconds + "s");
        }

        public void CommandFailed(CommandAnalyticInfo info)
        {
            Console.WriteLine("Command '" + info.Command.CommandText + "' failed");
        }
    }
}
