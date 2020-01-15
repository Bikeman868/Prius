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
        void ConnectionOpened(ConnectionAnalyticInfo info);

        /// <summary>
        /// This will be called whenever the application is finished using a database connection
        /// </summary>
        void ConnectionClosed(ConnectionAnalyticInfo info);

        /// <summary>
        /// This will be called whenever the application fails to establish a connection to the database
        /// </summary>
        void ConnectionFailed(ConnectionAnalyticInfo info);

        /// <summary>
        /// This will be called whenever a database command completes. Note that the elapsed
        /// time includes the time taken to read the result sets. If the application performs
        /// lengthy processing on each record this could be very long even if the database
        /// returned the records quickly.
        /// </summary>
        void CommandCompleted(CommandAnalyticInfo infos);

        /// <summary>
        /// This will be called whenever a command fails to execute for any reason
        /// </summary>
        void CommandFailed(CommandAnalyticInfo info);
    }

    /// <summary>
    /// This is used to pass analytic information back to the application when analytic
    /// reporting is enabled for the connection
    /// </summary>
    public class ConnectionAnalyticInfo
    {
        /// <summary>
        /// The name of the database driver used in this connection
        /// </summary>
        public string ServerType { get; set; }

        /// <summary>
        /// The name of the database server we are connected to
        /// </summary>
        public string RepositoryName { get; set; }

        /// <summary>
        /// The full database connection string used for the connection
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// True if this connection is from the connection pool
        /// </summary>
        public bool Pooled { get; set; }

        /// <summary>
        /// The total number of pooled/reusable connections to this server
        /// </summary>
        public int PooledCount { get; set; }

        /// <summary>
        /// The number of connection objects currently in use by the application
        /// </summary>
        public int ActiveCount { get; set; }
    }

    /// <summary>
    /// This is used to pass analytic information back to the application when analytic
    /// reporting is enabled for the connection
    /// </summary>
    public class CommandAnalyticInfo
    {
        /// <summary>
        /// The connection used to execute this command
        /// </summary>
        public IConnection Connection { get; set; }

        /// <summary>
        /// The command that was executed
        /// </summary>
        public ICommand Command { get; set; }

        /// <summary>
        /// The time taken to execute the command
        /// </summary>
        public double ElapsedSeconds { get; set; }
    }
}
