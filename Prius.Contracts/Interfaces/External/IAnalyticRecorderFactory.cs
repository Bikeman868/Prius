using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.External
{
    public interface IAnalyticRecorderFactory
    {
        /// <summary>
        /// This method is called for every command that is sent to the database.
        /// 
        /// If you want to record analytics for each command separately you can
        /// create and return a new IAnalyticRecorder each time this method is called.
        /// In this case you can return null in cases where you do not want to
        /// record analytics - for example if you only want to gather analytics for
        /// stored procedures that match a pattern.
        ///
        /// If you want to aggregate analytics across all database operations you can
        /// return the same instance of IAnalyticRecorder each time this method is
        /// called. In this case the implementation of IAnalyticRecorder must be fully
        /// thread safe.
        /// </summary>
        /// <param name="repositoryName">The name of the repository where a command is being executed</param>
        /// <param name="command">The command that is about to be executed</param>
        /// <returns>The object responsible for recording analytics or null if
        /// no analytics is required for this command execution</returns>
        IAnalyticRecorder Create(string repositoryName, ICommand command);
    }
}
