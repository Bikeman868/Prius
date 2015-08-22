namespace Prius.Contracts.Interfaces
{
    public interface IEnumerableDataFactory
    {
        IDisposableEnumerable<T> Create<T>(IContext context, IDataEnumerator<T> data) where T : class;
    }
}
