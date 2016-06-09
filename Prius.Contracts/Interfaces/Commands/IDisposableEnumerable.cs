using System;
using System.Collections.Generic;

namespace Prius.Contracts.Interfaces.Commands
{
    public interface IDisposableEnumerable<out T> : IDisposable, IEnumerable<T>
    {
    }
}
