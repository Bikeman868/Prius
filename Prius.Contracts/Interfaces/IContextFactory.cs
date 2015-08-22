namespace Prius.Contracts.Interfaces
{
    public interface IContextFactory
    {
        IContext Create(string repositoryName);
    }
}
