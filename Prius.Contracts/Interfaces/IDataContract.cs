using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces
{
    public interface IDataContract<T> where T: class
    {
        void AddMappings(ITypeDefinition<T> typeDefinition, string dataSetName);
        void SetCalculated(IDataReader dataReader, string dataSetName);
    }
}
