using System;

namespace Prius.Contracts.Interfaces
{
    public interface IDataEnumeratorFactory
    {
        IDataEnumerator<T> Create<T>(IDataReader reader, Action closeAction = null, string dataSetName = null, IFactory<T> dataContractFactory = null) where T : class;
    }
}
