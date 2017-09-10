namespace Prius.Contracts.Interfaces.External
{
    public interface ITraceWriterFactory
    {
        ITraceWriter Create(string repositoryName);
    }

}
