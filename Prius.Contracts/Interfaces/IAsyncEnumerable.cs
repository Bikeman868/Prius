using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.Contracts.Interfaces
{
    public interface IAsyncEnumerable<T>: IAsyncResult, IDisposable where T: class
    {
        IDataEnumerator<T> GetResults();
    }
}
