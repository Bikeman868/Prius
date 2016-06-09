using System;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.Contracts.Interfaces.Connections
{
    public interface IContext: IDisposable
    {
        void BeginTransaction();
        void Commit();
        void Rollback();

        // Command execution
        IConnection PrepareCommand(ICommand command);
        IAsyncResult BeginExecuteReader(ICommand command = null, AsyncCallback callback = null);
        IDataReader EndExecuteReader(IAsyncResult asyncResult);
        IAsyncResult BeginExecuteEnumerable(ICommand command = null, AsyncCallback callback = null);
        IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult, string dataSetName = null, IFactory<T> dataContractFactory = null) where T : class;
        IAsyncResult BeginExecuteNonQuery(ICommand command = null, AsyncCallback callback = null);
        long EndExecuteNonQuery(IAsyncResult asyncResult);
        IAsyncResult BeginExecuteScalar(ICommand command = null, AsyncCallback callback = null);
        T EndExecuteScalar<T>(IAsyncResult asyncResult);
    }
}
