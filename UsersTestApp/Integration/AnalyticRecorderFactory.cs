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

        public void ConnectionOpened(string serverType, string connectionString, bool fromPool, int pooledCount, int activeCount)
        {
            Console.WriteLine("Connection opened to '" + connectionString + "'. There are " + activeCount + " active connections");
        }

        public void ConnectionClosed(string serverType, string connectionString, bool toPool, int pooledCount, int activeCount)
        {
            Console.WriteLine("Connection closed to '" + connectionString + ". There are " + activeCount + " active connections");
        }

        public void ConnectionFailed(string serverType, string serverName)
        {
            Console.WriteLine("Failed to connect to database server '" + serverName + "'");
        }

        public void CommandCompleted(IConnection connection, ICommand command, double elapsedSeconds)
        {
            Console.WriteLine("Command '" + command.CommandText + "' completed in " + elapsedSeconds + "s");
        }

        public void CommandFailed(IConnection connection, ICommand command)
        {
            Console.WriteLine("Command '" + command.CommandText + "' failed");
        }
    }
}
