using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Interfaces.External
{
    /// <summary>
    /// Applications can implement this interface to gather analytic information
    /// about a database repository
    /// </summary>
    public interface IAnalyticRecorder
    {
        /// <summary>
        /// This will be called whenever the application requests a database connection
        /// </summary>
        /// <param name="serverType">The type of server that was connected to</param>
        /// <param name="connectionString">The connection string used to make the connection</param>
        /// <param name="fromPool">True if this was an already open connection taken from the connection pool</param>
        /// <param name="pooledCount">The count of connections remaining in the connection pool</param>
        /// <param name="activeCount">The count of connections actively being used by the application (including this one)</param>
        void ConnectionOpened(string serverType, string connectionString, bool fromPool, int pooledCount, int activeCount);

        /// <summary>
        /// This will be called whenever the application is finished using a database connection
        /// </summary>
        /// <param name="serverType">The type of server that was connected to</param>
        /// <param name="connectionString">The connection string used to make the connection</param>
        /// <param name="toPool">True if this connection was returned to the connection pool</param>
        /// <param name="pooledCount">The count of connections in the connection pool (after pooling this connection if appropriate)</param>
        /// <param name="activeCount">The count of connections actively being used by the application (not including this one)</param>
        void ConnectionClosed(string serverType, string connectionString, bool toPool, int pooledCount, int activeCount);

        /// <summary>
        /// This will be called whenever the application fails to establish a connection to the database
        /// </summary>
        /// <param name="serverType">The type of server</param>
        /// <param name="serverName">The name of the server</param>
        void ConnectionFailed(string serverType, string serverName);

        /// <summary>
        /// This will be called whenever a database command completes. Note that the elapsed
        /// time includes the time taken to read the result sets. If the application performs
        /// lengthy processing on each record this could be very long even if the database
        /// returned the records quickly.
        /// </summary>
        void CommandCompleted(IConnection connection, ICommand command, double elapsedSeconds);

        /// <summary>
        /// This will be called whenever a command fails to execute for any reason
        /// </summary>
        void CommandFailed(IConnection connection, ICommand command);
    }
}
