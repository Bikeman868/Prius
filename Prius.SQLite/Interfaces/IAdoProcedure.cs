using Prius.Contracts.Interfaces;
using Prius.SQLite.Procedures;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// Stored procedure classes that use the ADO.Net driver
    /// in System.Data.SQLite should implement this interface.
    /// They also need to be decorated with the [Procedure]
    /// attribute to be recognised.
    /// </summary>
    public interface IAdoProcedure: IProcedure
    {
        IDataReader Execute(AdoExecutionContext context);
    }
}
