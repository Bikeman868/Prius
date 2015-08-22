using Prius.Contracts.Interfaces;

namespace Prius.Orm.Orm
{
    public class EnumerableDataFactory : IEnumerableDataFactory
    {
        public IEnumerableDataFactory Initialize()
        {
            return this;
        }

        public IDisposableEnumerable<T> Create<T>(IContext context, IDataEnumerator<T> data) where T : class
        {
            return new EnumerableData<T>().Initialize(context, data);
        }
    }
}
