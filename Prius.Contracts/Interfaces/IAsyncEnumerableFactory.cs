using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.Contracts.Interfaces
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
