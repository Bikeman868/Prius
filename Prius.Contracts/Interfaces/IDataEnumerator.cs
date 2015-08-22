using System;
using System.Collections.Generic;

namespace Prius.Contracts.Interfaces
{
    public interface IDataEnumerator<T> : IDisposable, IEnumerable<T> where T: class
    {
        bool IsServerOffline { get; }

        Exception ServerOfflineException { get; }
    }
}
