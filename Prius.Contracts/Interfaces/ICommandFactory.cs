using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces
{
    public interface ICommandFactory
    {
        ICommand CreateStoredProcedure(string procedureName, int? timeoutSeconds = null);
        ICommand CreateSql(string sql, int? timeoutSeconds = null);
    }
}
