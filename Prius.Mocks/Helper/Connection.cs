using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;

namespace Prius.Mocks.Helper
{
    internal class Connection : IConnection
    {
        private string _repositoryName;
        private ICommand _command;
        private Dictionary<string, JArray> _mockedData;
        private Dictionary<string, Func<JArray, List<IParameter>, JArray>> _filters;
        private Dictionary<string, Func<List<IParameter>, object>> _scalarFunctions;
        private Dictionary<string, Func<List<IParameter>, long>> _nonQueryFunctions;

        public object RepositoryContext { get; set; }

        public IConnection Initialize(
            string repositoryName, 
            Dictionary<string, JArray> mockedData,
            Dictionary<string, Func<JArray, List<IParameter>, JArray>> filters,
            Dictionary<string, Func<List<IParameter>, object>> scalarFunctions,
            Dictionary<string, Func<List<IParameter>, long>> nonQueryFunctions)
        {
            _repositoryName = repositoryName;
            _mockedData = mockedData;
            _filters = filters;
            _scalarFunctions = scalarFunctions;
            _nonQueryFunctions = nonQueryFunctions;
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

            var data = (JArray) null;
            if (_mockedData.ContainsKey(procedureName))
                data = _mockedData[procedureName];

            if (_filters.ContainsKey(procedureName))
                data = _filters[procedureName](data, command.GetParameters().ToList());

            return new DataReader().Initialize(procedureName, data);
        }

        public IAsyncResult BeginExecuteEnumerable(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class
        {
            var command = asyncResult.AsyncState as ICommand;
            var procedureName = command.CommandText.ToLower();

            var data = (JArray)null;
            if (_mockedData.ContainsKey(procedureName))
                data = _mockedData[procedureName];

            if (_filters.ContainsKey(procedureName))
                data = _filters[procedureName](data, command.GetParameters().ToList());

            return new DataEnumerator<T>().Initialize(procedureName, data);
        }

        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback)
        {
            return new SyncronousResult(_command, callback);
        }

        public long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            var command = asyncResult.AsyncState as ICommand;
            var procedureName = command.CommandText.ToLower();

            if (_nonQueryFunctions.ContainsKey(procedureName))
                return _nonQueryFunctions[procedureName](command.GetParameters().ToList());

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

            if (_scalarFunctions.ContainsKey(procedureName))
                return (T)_scalarFunctions[procedureName](command.GetParameters().ToList());

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
