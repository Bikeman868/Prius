using Prius.Contracts.Interfaces.External;

namespace UsersTestApp.Integration
{
    internal class TraceWriterFactory: ITraceWriterFactory
    {
        public ITraceWriter Create(string repositoryName)
        {
            return new TraceWriter(repositoryName);
        }
    }
}
