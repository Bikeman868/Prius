using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.Contracts.Interfaces
{
    public interface IDisposableEnumerable<out T> : IDisposable, IEnumerable<T>
    {
    }
}
