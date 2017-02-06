using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
{
    public interface INativeQueryRunner
    {
        void ExecuteNonQuery(NativeExecutionContext context, string sql);
        object ExecuteReader(NativeExecutionContext context, string sql);
    }
}
