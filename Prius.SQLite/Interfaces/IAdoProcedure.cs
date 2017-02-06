using Prius.Contracts.Interfaces;
using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
{
    public interface IAdoProcedure: IProcedure
    {
        IDataReader Execute(AdoExecutionContext context);
    }
}
