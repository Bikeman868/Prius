using System;
using System.Threading;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.Factory;
using Prius.Orm.Utility;

namespace Prius.Orm.Connections
{
    public abstract class Connection: Disposable, IConnection
    {
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;

        public object RepositoryContext { get; set; }

        public abstract void BeginTransaction();
        public abstract void Commit();
        public abstract void Rollback();
        public abstract void SetCommand(ICommand command);

        public abstract IAsyncResult BeginExecuteReader(AsyncCallback callback);
        public abstract IAsyncResult BeginExecuteNonQuery(AsyncCallback callback);
        public abstract IAsyncResult BeginExecuteScalar(AsyncCallback callback);

        public abstract IDataReader EndExecuteReader(IAsyncResult asyncResult);
        public abstract long EndExecuteNonQuery(IAsyncResult asyncResult);
        public abstract T EndExecuteScalar<T>(IAsyncResult asyncResult);

        protected Connection(IDataEnumeratorFactory dataEnumeratorFactory)
        {
            _dataEnumeratorFactory = dataEnumeratorFactory;
        }

        public IAsyncResult BeginExecuteEnumerable(AsyncCallback callback)
        {
            return BeginExecuteReader(callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T: class
        {
            var reader = EndExecuteReader(asyncResult);
            return _dataEnumeratorFactory.Create<T>(reader, reader.Dispose);
        }

        protected class AsyncContext
        {
            public object Result;
            public bool InitiallyClosed;
            public long StartTime;
        }

        protected class SyncronousResult : IAsyncResult
        {
            public WaitHandle AsyncWaitHandle { get; private set; }
            public object AsyncState { get; private set;}
            public bool CompletedSynchronously { get { return true; } }
            public bool IsCompleted { get { return true; } }

            public SyncronousResult(AsyncContext asyncContext, AsyncCallback callback)
            {
                AsyncState = asyncContext;
                AsyncWaitHandle = new ManualResetEvent(true);
                if (callback != null) callback(this);
            }
        }
    }
}
