namespace Prius.Contracts.Interfaces.External
{
    public interface IFactory
    {
        T Create<T>() where T: class;
    }

    public interface IFactory<out T> where T : class
    {
        T Create();
    }
}
