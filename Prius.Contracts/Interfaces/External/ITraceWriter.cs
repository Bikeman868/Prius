namespace Prius.Contracts.Interfaces.External
{
    public interface ITraceWriter
    {
        void SetCluster(string clusterName);
        void SetDatabase(string databaseName);
        void SetProcedure(string storedProcedureName);

        void WriteLine(string message);
    }
}
