using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IEnumerableDataFactory
    {
        IDisposableEnumerable<T> Create<T>(IContext context, IDataEnumerator<T> data) where T : class;
    }
}
