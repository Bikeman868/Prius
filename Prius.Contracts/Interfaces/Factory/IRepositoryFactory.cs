using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IRepositoryFactory
    {
        IRepository Create(string repositoryName);
    }
}
