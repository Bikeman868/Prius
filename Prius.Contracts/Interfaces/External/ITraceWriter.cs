using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.External
{
    /// <summary>
    /// Custom trace writer factories should return one of these for each
    /// database context.
    /// </summary>
    public interface ITraceWriter
    {
        /// <summary>
        /// This is called when the database connection is assigned to a
        /// cluster of servers by the health checker and load balancer algorithm
        /// </summary>
        void SetCluster(string clusterName);

        /// <summary>
        /// This is called once for each database connection to let you know
        /// the name of the database that is being connected to
        /// </summary>
        void SetDatabase(string databaseName);

        /// <summary>
        /// This is called at the start of the sequence of events for calling
        /// a stored procedure. If an ad-hoc query is being executed this will
        /// be the text of the query instead
        /// </summary>
        void SetProcedure(string storedProcedureName);

        /// <summary>
        /// When this is called you should write the text to a log that is
        /// specific to the database connection
        /// </summary>
        void WriteLine(string message);
    }

    /// <summary>
    /// An etended version of the original trace writer. If your trace writer
    /// factory implements this newer version is will be used, otherwise Prius
    /// will use the original ITraceWriter
    /// </summary>
    public interface ITraceWriter2: ITraceWriter
    {
        /// <summary>
        /// This is called once for each parameter that is passed to the
        /// stored procedure
        /// </summary>
        void SetParameter(IParameter parameter);
    }
}
