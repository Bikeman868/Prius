namespace Prius.Contracts.Interfaces
{
    public interface IFactory
    {
        T Create<T>();
    }

    public interface IFactory<out T>
    {
        T Create();
    }
}
