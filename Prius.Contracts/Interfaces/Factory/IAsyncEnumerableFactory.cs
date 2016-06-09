using System;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IAsyncEnumerableFactory
    {
        IAsyncEnumerable<T> Create<T>(
            IContext context, 
            ICommand command, 
            IAsyncResult asyncResult, 
            string dataSetName = null, 
            IFactory<T> dataContractFactory = null)
            where T: class;
    }
}
