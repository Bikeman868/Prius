using System;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IDataEnumeratorFactory
    {
        IDataEnumerator<T> Create<T>(IDataReader reader, Action closeAction = null, string dataSetName = null, IFactory<T> dataContractFactory = null) where T : class;
    }
}
