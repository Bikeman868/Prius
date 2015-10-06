using System;
using Prius.Contracts.Interfaces;

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
