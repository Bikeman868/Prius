using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IRepositoryFactory
    {
        /// <summary>
        /// Gets a repository instance for a configured repository name. May create
        /// a new instance or return a thread-safe singleton
        /// </summary>
        IRepository Create(string repositoryName);

        /// <summary>
        /// Enables tracing on any new repositories that are opened/created
        /// </summary>
        void EnableTracing(ITraceWriterFactory traceWriterFactory);

        /// <summary>
        /// Enabled analytic recording on any new repositories that are opened/created
        /// </summary>
        void EnableAnalyticRecording(IAnalyticRecorderFactory analyticRecorder);
    }
}
