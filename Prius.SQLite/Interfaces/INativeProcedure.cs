using Prius.Contracts.Interfaces;
using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
{
    public interface INativeProcedure : IProcedure
    {
        IDataReader Execute(NativeExecutionContext context);
    }
}
