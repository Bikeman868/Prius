using Prius.SqLite.Procedures;

namespace Prius.SqLite.Interfaces
{
    /// <summary>
    /// Provides a query execution mechanism thet talks directly to
    /// the SqLite engine
    /// </summary>
    public interface INativeQueryRunner
    {
        void ExecuteNonQuery(NativeExecutionContext context, string sql);
        object ExecuteReader(NativeExecutionContext context, string sql);
    }
}
