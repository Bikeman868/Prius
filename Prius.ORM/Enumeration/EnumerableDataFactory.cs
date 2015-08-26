using Prius.Contracts.Interfaces;

namespace Prius.Orm.Enumeration
{
    public class EnumerableDataFactory : IEnumerableDataFactory
    {
        public IDisposableEnumerable<T> Create<T>(IContext context, IDataEnumerator<T> data) where T : class
        {
            return new EnumerableData<T>().Initialize(context, data);
        }
    }
}
