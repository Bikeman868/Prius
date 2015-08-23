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
        private readonly Dictionary<string, JArray> _mockedData = new Dictionary<string,JArray>();

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

        protected override void SetupMock(IMockProducer mockProducer, Mock<IContextFactory> mock)
        {
            mock.Setup(m =>
                m.Create(It.IsAny<string>()))
                .Returns((string repositoryName) => new Context().Initialize(repositoryName, _mockedData));
        }

        private class Context : IContext
        {
            private string _repositoryName;
            private Dictionary<string, JArray> _mockedData;

            public IContext Initialize(string repositoryName, Dictionary<string, JArray> mockedData)
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

            public bool ReportErrors { get; set; }

            public IConnection PrepareCommand(ICommand command)
            {
                var connection = new Connection().Initialize(_repositoryName, _mockedData);

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
