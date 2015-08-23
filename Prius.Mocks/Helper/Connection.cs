using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;

namespace Prius.Mocks.Helper
{
    internal class Connection : IConnection
    {
        private string _repositoryName;
        private ICommand _command;
        private Dictionary<string, JArray> _mockedData;

        public object RepositoryContext { get; set; }

        public IConnection Initialize(string repositoryName, Dictionary<string, JArray> mockedData)
        {
            _repositoryName = repositoryName;
            _mockedData = mockedData;
            return this;
        }

        public void BeginTransaction()
        {
        }

        public void Commit()
        {
        }

        public void Rollback()
        {
        }

        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        public IAsyncResult BeginExecuteReader(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            var command = asyncResult.AsyncState as ICommand;
            var procedureName = command.CommandText.ToLower();

            if (_mockedData.ContainsKey(procedureName))
                return new DataReader().Initialize(procedureName, _mockedData[procedureName]);

            return new DataReader().Initialize(procedureName, null);
        }

        public IAsyncResult BeginExecuteEnumerable(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class
        {
            var command = asyncResult.AsyncState as ICommand;
            var procedureName = command.CommandText.ToLower();

            if (_mockedData.ContainsKey(procedureName))
                return new DataEnumerator<T>().Initialize(procedureName, _mockedData[procedureName]);

            return new DataEnumerator<T>().Initialize(procedureName, null);
        }

        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            var command = asyncResult.AsyncState as ICommand;
            var procedureName = command.CommandText.ToLower();

            return 0;
        }

        public IAsyncResult BeginExecuteScalar(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public T EndExecuteScalar<T>(IAsyncResult asyncResult)
        {
            var command = asyncResult.AsyncState as ICommand;
            var procedureName = command.CommandText.ToLower();

            return default(T);
        }

        public bool IsReusable { get { return false; } }

        public bool IsDisposing { get { return false; } }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }
}
