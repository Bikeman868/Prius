using Prius.Contracts.Interfaces;
using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
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
