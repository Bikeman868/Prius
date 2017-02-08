using Prius.Contracts.Interfaces;
using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
{
    /// <summary>
    /// Stored procedure classes that talk directly to the 
    /// SqLite engine should implement this interface.
    /// They also need to be decorated with the [Procedure]
    /// attribute to be recognised.
    /// </summary>
    public interface INativeProcedure : IProcedure
    {
        IDataReader Execute(NativeExecutionContext context);
    }
}
