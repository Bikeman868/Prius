namespace Prius.Contracts.Interfaces
{
    public interface IRepositoryFactory
    {
        IRepository Create(string repositoryName);
    }
}
