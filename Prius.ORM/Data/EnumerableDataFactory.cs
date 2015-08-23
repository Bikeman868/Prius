using Prius.Contracts.Interfaces;

namespace Prius.Orm.Data
{
    public class EnumerableDataFactory : IEnumerableDataFactory
    {
        public IDisposableEnumerable<T> Create<T>(IContext context, IDataEnumerator<T> data) where T : class
        {
            return new EnumerableData<T>().Initialize(context, data);
        }
    }
}
