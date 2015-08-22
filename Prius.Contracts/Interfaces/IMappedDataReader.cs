namespace Prius.Contracts.Interfaces
{
    public interface IMappedDataReader<TDataContract>: IDataReader where TDataContract : class
    {
        TDataContract Map();
        TDataContract Fill(TDataContract dataContract);
    }
}
