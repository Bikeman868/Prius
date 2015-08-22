using System;

namespace Prius.Contracts.Interfaces
{
    public interface IConnection : IDisposable
    {
        object RepositoryContext { get; set; }

        void BeginTransaction();
        void Commit();
        void Rollback();

        void SetCommand(ICommand command);

        IAsyncResult BeginExecuteReader(AsyncCallback callback = null);
        IDataReader EndExecuteReader(IAsyncResult asyncResult);

        IAsyncResult BeginExecuteEnumerable(AsyncCallback callback = null);
        IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class;

        IAsyncResult BeginExecuteNonQuery(AsyncCallback callback = null);
        long EndExecuteNonQuery(IAsyncResult asyncResult);

        IAsyncResult BeginExecuteScalar(AsyncCallback callback = null);
        T EndExecuteScalar<T>(IAsyncResult asyncResult);
    }

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
