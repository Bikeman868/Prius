using System;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;

namespace Prius.Orm.Enumeration
{
    public class DataEnumeratorFactory : IDataEnumeratorFactory
    {
        private readonly IMapper _mapper;

        public DataEnumeratorFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        public IDataEnumerator<T> Create<T>(IDataReader reader, Action closeAction, string dataSetName, IFactory<T> dataContractFactory) where T : class
        {
            return new DataEnumerator<T>(_mapper).Initialize(reader, closeAction, dataSetName, dataContractFactory);
        }

        public IDataEnumerator<T> Create<T>() where T : class
        {
            return new DummyDataEnumerator<T>().Initialize();
        }
    }
}
