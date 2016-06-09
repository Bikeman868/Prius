using System;

namespace Prius.Contracts.Interfaces.Commands
{
    public interface IAsyncEnumerable<T>: IAsyncResult, IDisposable where T: class
    {
        IDataEnumerator<T> GetResults();
    }
}
