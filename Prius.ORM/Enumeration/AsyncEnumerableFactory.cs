using System;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;

namespace Prius.Orm.Enumeration
{
    public class AsyncEnumerableFactory : IAsyncEnumerableFactory
    {
        public IAsyncEnumerable<T> Create<T>(
            IContext context, 
            ICommand command, 
            IAsyncResult asyncResult, 
            string dataSetName, 
            IFactory<T> dataContractFactory) 
            where T : class
        {
            return new AsyncEnumerable<T>().Initialize(context, command, asyncResult, dataSetName, dataContractFactory);
        }
    }
}
