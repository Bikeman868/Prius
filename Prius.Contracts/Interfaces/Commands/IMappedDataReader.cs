namespace Prius.Contracts.Interfaces.Commands
{
    public interface IMappedDataReader<TDataContract>: IDataReader where TDataContract : class
    {
        TDataContract Map();
        TDataContract Fill(TDataContract dataContract);
    }
}
