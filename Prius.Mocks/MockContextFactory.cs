using System;
using System.Collections.Generic;
using Moq.Modules;
using Prius.Contracts.Interfaces;
using Moq;
using Newtonsoft.Json.Linq;
using Prius.Mocks.Helper;

namespace Prius.Mocks
{
    public class MockContextFactory : MockImplementationProvider<IContextFactory>
    {
        private readonly Dictionary<string, JArray> _mockedData = new Dictionary<string, JArray>();
        private readonly Dictionary<string, Func<JArray, List<IParameter>, JArray>> _filterFunctions = new Dictionary<string, Func<JArray, List<IParameter>, JArray>>();
        private readonly Dictionary<string, Func<List<IParameter>, object>> _scalarFunctions = new Dictionary<string, Func<List<IParameter>, object>>();
        private readonly Dictionary<string, Func<List<IParameter>, long>> _nonQueryFunctions = new Dictionary<string, Func<List<IParameter>, long>>();

        public void AddMockData(string procedureName, JArray data)
        {
            procedureName = procedureName.ToLower();

            if (!_mockedData.ContainsKey(procedureName))
                _mockedData[procedureName] = new JArray();

            var existingRows = _mockedData[procedureName];
            foreach (var element in data)
                existingRows.Add(element);
        }

        public void ClearMockData(string procedureName)
        {
            procedureName = procedureName.ToLower();

            if (!_mockedData.ContainsKey(procedureName))
                _mockedData[procedureName] = new JArray();

            _mockedData[procedureName].Clear();
        }

        public void SetFilter(string procedureName, Func<JArray, List<IParameter>, JArray> filter)
        {
            procedureName = procedureName.ToLower();
            _filterFunctions[procedureName] = filter;
        }

        public void SetScalar(string procedureName, Func<List<IParameter>, object> scalar)
        {
            procedureName = procedureName.ToLower();
            _scalarFunctions[procedureName] = scalar;
        }

        public void SetNonQuery(string procedureName, Func<List<IParameter>, long> nonQuery)
        {
            procedureName = procedureName.ToLower();
            _nonQueryFunctions[procedureName] = nonQuery;
        }

        protected override void SetupMock(IMockProducer mockProducer, Mock<IContextFactory> mock)
        {
            mock.Setup(m => m.Create(It.IsAny<string>()))
                .Returns((string repositoryName) => new Context().Initialize(repositoryName, _mockedData, _filterFunctions, _scalarFunctions, _nonQueryFunctions));
        }

        private class Context : IContext
        {
            private string _repositoryName;
            private Dictionary<string, JArray> _mockedData;
            private Dictionary<string, Func<JArray, List<IParameter>, JArray>> _filters;
            private Dictionary<string, Func<List<IParameter>, object>> _scalarFunctions;
            private Dictionary<string, Func<List<IParameter>, long>> _nonQueryFunctions;

            public IContext Initialize(
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

            public bool ReportErrors { get; set; }

            public IConnection PrepareCommand(ICommand command)
            {
                var connection = new Connection().Initialize(_repositoryName, _mockedData, _filters, _scalarFunctions, _nonQueryFunctions);

                if (command != null)
                    connection.SetCommand(command);

                return connection;
            }

            public IAsyncResult BeginExecuteReader(ICommand command, AsyncCallback callback)
            {
                return new SyncronousResult(command, callback);
            }

            public IDataReader EndExecuteReader(IAsyncResult asyncResult)
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteReader(asyncResult);
            }

            public IAsyncResult BeginExecuteEnumerable(ICommand command, AsyncCallback callback)
            {
                return PrepareCommand(command).BeginExecuteEnumerable(callback);
            }

            public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult, string dataSetName, IFactory<T> dataContractFactory) where T : class
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteEnumerable<T>(asyncResult);
            }

            public IAsyncResult BeginExecuteNonQuery(ICommand command, AsyncCallback callback)
            {
                return new SyncronousResult(command, callback);
            }

            public long EndExecuteNonQuery(IAsyncResult asyncResult)
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteNonQuery(asyncResult);
            }

            public IAsyncResult BeginExecuteScalar(ICommand command, AsyncCallback callback)
            {
                return new SyncronousResult(command, callback);
            }

            public T EndExecuteScalar<T>(IAsyncResult asyncResult)
            {
                return PrepareCommand(asyncResult.AsyncState as ICommand).EndExecuteScalar<T>(asyncResult);
            }

            public bool IsReusable { get { return false; } }

            public bool IsDisposing { get { return false; } }

            private bool IsDisposed { get; set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
