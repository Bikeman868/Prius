using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.Factory;

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
