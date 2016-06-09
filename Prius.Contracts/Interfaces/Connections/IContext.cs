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

    public static class IContextExtensions
    {
        public static IDataReader ExecuteReader(this IContext context, ICommand command)
        {
            return context.EndExecuteReader(context.BeginExecuteReader(command));
        }

        public static IDataEnumerator<T> ExecuteEnumerable<T>(this IContext context, ICommand command, string dataSetName = null, IFactory<T> dataContractFactory = null) where T : class
        {
            return context.EndExecuteEnumerable<T>(context.BeginExecuteEnumerable(command), dataSetName, dataContractFactory);
        }

        public static long ExecuteNonQuery(this IContext context, ICommand command)
        {
            return context.EndExecuteNonQuery(context.BeginExecuteNonQuery(command));
        }

        public static T ExecuteScalar<T>(this IContext context, ICommand command)
        {
            return context.EndExecuteScalar<T>(context.BeginExecuteScalar(command));
        }
    }
}
