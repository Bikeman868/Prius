namespace Prius.Contracts.Interfaces
{
    public interface ICommandFactory
    {
        ICommand CreateStoredProcedure(string procedureName, int timeoutSeconds = 5);
        ICommand CreateSql(string sql, int timeoutSeconds = 5);
    }
}
