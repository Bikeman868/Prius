using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IRepositoryFactory
    {
        IRepository Create(string repositoryName);
        void EnableTracing(ITraceWriterFactory traceWriterFactory);
    }
}
