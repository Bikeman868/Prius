using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces
{
    public interface IMapper
    {
        TDataContract Map<TDataContract>(IDataReader dataReader, string dataSetName = null, IFactory<TDataContract> dataContractFactory = null) where TDataContract : class;
        void Fill<TDataContract>(TDataContract dataContract, IDataReader dataReader, string dataSetName = null) where TDataContract : class;
        IMappedDataReader<TDataContract> GetMappedDataReader<TDataContract>(IDataReader dataReader, string dataSetName = null, IFactory<TDataContract> dataContractFactory = null) where TDataContract : class;
    }
}
