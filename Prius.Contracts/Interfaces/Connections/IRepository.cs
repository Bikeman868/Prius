using System;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Connections
{
    public interface IRepository : IDisposable
    {
        /// <summary>
        /// Each repository must have a unique name. This is the name used within
        /// application code to define which database cluster to send requests to
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns a disposable connection to the repository
        /// </summary>
        IConnection GetConnection(ICommand command);

        /// <summary>
        /// This will be called whenever a database request completes successfully
        /// </summary>
        void RecordSuccess(IConnection connection, double elapsedSeconds);

        /// <summary>
        /// This will be called whenever a database request fails for any reason
        /// </summary>
        void RecordFailure(IConnection connection);

        /// <summary>
        /// Tracing can be enabled by calling this method on the repository. When
        /// tracing is enabled detailed debugging information can be captured to
        /// help developers track down issues. You should not leave tracing enabled
        /// in a busy production environment
        /// </summary>
        void EnableTracing(ITraceWriterFactory traceWriterFactory);

        /// <summary>
        /// Analytic recording can be enabled by calling this method on the repository.
        /// When analytic recording is enabled the repository provides information
        /// about database operations that can be used to measure and alert on database
        /// activity by the application
        /// </summary>
        void EnableAnalyticRecording(IAnalyticRecorderFactory analyticRecorderFactory);
    }
}
