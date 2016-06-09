using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.Contracts.Interfaces
{
    public static class IConnectionExtensions
    {
        public static IDataReader ExecuteReader(this IConnection connection)
        {
            return connection.EndExecuteReader(connection.BeginExecuteReader());
        }

        public static IDataEnumerator<T> ExecuteEnumerable<T>(this IConnection connection) where T : class
        {
            return connection.EndExecuteEnumerable<T>(connection.BeginExecuteEnumerable());
        }

        public static long ExecuteNonQuery(this IConnection connection)
        {
            return connection.EndExecuteNonQuery(connection.BeginExecuteNonQuery());
        }

        public static T ExecuteScalar<T>(this IConnection connection)
        {
            return connection.EndExecuteScalar<T>(connection.BeginExecuteScalar());
        }
    }
}
