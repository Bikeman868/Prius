using System;
using System.Collections.Generic;
using System.Linq;
using Moq.Modules;
using Prius.Contracts.Interfaces;
using Moq;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Collections;

namespace Prius.Mocks
{
    public class MockContextFactory : MockImplementationProvider<IContextFactory>
    {
        private Dictionary<string, JArray> _mockedData = new Dictionary<string,JArray>();

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

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class Connection : IConnection
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

        private class SyncronousResult : IAsyncResult
        {
            public WaitHandle AsyncWaitHandle { get; private set; }
            public object AsyncState { get; private set; }
            public bool CompletedSynchronously { get { return true; } }
            public bool IsCompleted { get { return true; } }

            public SyncronousResult(object asyncContext, AsyncCallback callback)
            {
                AsyncState = asyncContext;
                AsyncWaitHandle = new ManualResetEvent(true);
                if (callback != null) callback(this);
            }
        }

        private class DataReader: IDataReader
        {
            private JArray _data;
            private int _rowNumber;
            private List<JProperty> _schema;

            public IDataReader Initialize(string dataShapeName, JArray data)
            {
                DataShapeName = dataShapeName;
                _data = data;
                _rowNumber = -1;

                if (data != null || data.Count > 0)
                {
                    if (data.First is JObject)
                        _schema = (data.First as JObject).Properties().ToList();
                }

                return this;
            }

            public string DataShapeName { get; private set; }

            public int FieldCount 
            {
                get
                {
                    return _schema == null ? 0 : _schema.Count;
                }
            }

            public bool IsServerOffline { get { return false; } }

            public Exception ServerOfflineException { get { return null; } }

            public object this[int fieldIndex]
            {
                get
                {
                    var fieldName = GetFieldName(fieldIndex);
                    return this[fieldName];
                }
            }

            public object this[string fieldName]
            {
                get 
                {
                    return ((_data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value;
                }
            }

            public string GetFieldName(int fieldIndex)
            {
                if (fieldIndex < 0) return null;

                var property = _schema.Skip(fieldIndex).FirstOrDefault();
                if (property == null) return null;
                return property.Name;
            }

            public int GetFieldIndex(string fieldName)
            {
                if (_schema == null) return -1;

                for (var i = 0; i < _schema.Count; i++)
                {
                    if (_schema[i].Name.ToLower() == fieldName.ToLower())
                        return i;
                }
                return -1;
            }

            public bool IsNull(int fieldIndex)
            {
                var fieldName = GetFieldName(fieldIndex);
                var value = (_data[_rowNumber] as JObject).GetValue(fieldName) as JValue;
                if (value == null) return true;
                return value.Value == null;
            }

            public bool Read()
            {
                if (_rowNumber == _data.Count - 1)
                    return false;

                _rowNumber++;
                return true;
            }

            public bool NextResult()
            {
                return Read();
            }

            public T Get<T>(int fieldIndex, T defaultValue)
            {
                var fieldName = GetFieldName(fieldIndex);
                if (fieldName == null) return defaultValue;

                return ((_data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value<T>();
            }

            public T Get<T>(string fieldName, T defaultValue)
            {
                return ((_data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value<T>();
            }

            public object Get(int fieldIndex, object defaultValue, Type type)
            {
                var fieldName = GetFieldName(fieldIndex);
                if (fieldName == null) return defaultValue;

                return ((_data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value;
            }

            public bool IsReusable { get { return false; } }

            public bool IsDisposing { get { return false; } }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class DataEnumerator<T> : IDataEnumerator<T> where T : class
        {
            private JArray _data;

            public IDataEnumerator<T> Initialize(string dataShapeName, JArray data)
            {
                _data = data;
                return this;
            }

            public bool IsServerOffline { get { return false; } }

            public Exception ServerOfflineException { get { return null; } }

            public bool IsReusable { get { return false; } }

            public bool IsDisposing { get { return false; } }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (_data == null)
                    return new List<T>().GetEnumerator();

                return _data.Select(e => ((JObject)e).ToObject<T>()).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
