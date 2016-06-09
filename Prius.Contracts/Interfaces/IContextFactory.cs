using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Interfaces
{
    public interface IContextFactory
    {
        IContext Create(string repositoryName);
    }
}
