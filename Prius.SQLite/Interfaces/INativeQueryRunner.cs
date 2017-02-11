using Prius.SQLite.Procedures;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// Provides a query execution mechanism thet talks directly to
    /// the SQLite engine
    /// </summary>
    public interface INativeQueryRunner
    {
        void ExecuteNonQuery(NativeExecutionContext context, string sql);
        object ExecuteReader(NativeExecutionContext context, string sql);
    }
}
