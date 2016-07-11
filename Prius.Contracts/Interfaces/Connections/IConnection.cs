using System;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.Connections
{
    public interface IConnection : IDisposable
    {
        object RepositoryContext { get; set; }

        void BeginTransaction();
        void Commit();
        void Rollback();

        void SetCommand(ICommand command);

        IDataReader ExecuteReader();
        IDataEnumerator<T> ExecuteEnumerable<T>() where T : class;
        long ExecuteNonQuery();
        T ExecuteScalar<T>();

        IAsyncResult BeginExecuteReader(AsyncCallback callback = null);
        IDataReader EndExecuteReader(IAsyncResult asyncResult);

        IAsyncResult BeginExecuteEnumerable(AsyncCallback callback = null);
        IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class;

        IAsyncResult BeginExecuteNonQuery(AsyncCallback callback = null);
        long EndExecuteNonQuery(IAsyncResult asyncResult);

        IAsyncResult BeginExecuteScalar(AsyncCallback callback = null);
        T EndExecuteScalar<T>(IAsyncResult asyncResult);
    }
}
