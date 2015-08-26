using System;
using Prius.Contracts.Interfaces;

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
