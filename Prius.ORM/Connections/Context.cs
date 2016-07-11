using System;
using System.Threading;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Orm.Utility;

namespace Prius.Orm.Connections
{
    public class Context : Disposable, IContext
    {
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;
        private readonly IErrorReporter _errorReporter;

        private IRepository _repository;
        private IConnection _connection;
        private bool _isPrepared;

        #region Lifetime

        public Context(
            IDataEnumeratorFactory dataEnumeratorFactory, 
            IErrorReporter errorReporter,
            IRepository repository)
        {
            _dataEnumeratorFactory = dataEnumeratorFactory;
            _errorReporter = errorReporter;
            _repository = repository;
            _connection = null;
            _isPrepared = false;
        }

        protected override void Dispose(bool destructor)
        {
            if (_connection != null)
                _connection.Dispose();
            base.Dispose(destructor);
        }

        #endregion

        #region Transactions

        public void BeginTransaction()
        {
            if (_connection != null)
                _connection.BeginTransaction();
        }

        public void Commit()
        {
            if (_connection != null)
                _connection.Commit();
        }

        public void Rollback()
        {
            if (_connection != null)
                _connection.Rollback();
        }

        #endregion

        #region Preparing commands

        public IConnection PrepareCommand(ICommand command)
        {
            if (_connection == null)
                _connection = _repository.GetConnection(command);
            else
                _connection.SetCommand(command);
            _isPrepared = true;
            return _connection;
        }

        #endregion

        #region ExecuteReader

        public IAsyncResult BeginExecuteReader(ICommand command, AsyncCallback callback)
        {
            if (command != null) PrepareCommand(command);
            if (!_isPrepared)
                throw new ApplicationException(
                    "You must call the PrepareCommand() method before calling BeginExecuteReader() " +
                    "or pass the command to the BeginExecuteReader() method");
            try
            {
                if (_connection == null)
                    return new SyncronousResult(callback, new Exception("Empty connection."));
                return _connection.BeginExecuteReader();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to BeginExecuteReader on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        public IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            try
            {
                if (asyncResult is SyncronousResult && asyncResult.AsyncState is Exception)
                    throw (Exception)asyncResult.AsyncState;
                return _connection.EndExecuteReader(asyncResult);
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to EndExecuteReader on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        public IDataReader ExecuteReader(ICommand command)
        {
            if (command != null) PrepareCommand(command);
            if (!_isPrepared) 
                throw new ApplicationException(
                    "You must call the PrepareCommand() method before calling ExecuteReader() " +
                    "or pass the command to the ExecuteReader() method");
            try
            {
                if (_connection == null)
                    throw new Exception("Empty connection.");
                return _connection.ExecuteReader();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to ExecuteReader on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        #endregion

        #region ExecuteEnumerable

        public IAsyncResult BeginExecuteEnumerable(ICommand command, AsyncCallback callback)
        {
            return BeginExecuteReader(command, callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult, string dataSetName = null, IFactory<T> dataContractFactory = null) where T : class
        {
            var reader = EndExecuteReader(asyncResult);
            return _dataEnumeratorFactory.Create(reader, reader.Dispose, dataSetName, dataContractFactory);
        }

        public IDataEnumerator<T> ExecuteEnumerable<T>(
            ICommand command, 
            string dataSetName = null, 
            IFactory<T> dataContractFactory = null) where T : class
        {
            var reader = ExecuteReader(command);
            return _dataEnumeratorFactory.Create(reader, reader.Dispose, dataSetName, dataContractFactory);
        }

        #endregion

        #region ExecuteNonQuery

        public IAsyncResult BeginExecuteNonQuery(ICommand command, AsyncCallback callback)
        {
            if (command != null) PrepareCommand(command);
            if (!_isPrepared) 
                throw new ApplicationException(
                    "You must call the PrepareCommand() method before calling BeginExecuteNonQuery() "+
                    "or pass the command to the BeginExecuteNonQuery() method");
            try
            {
                if (_connection == null)
                    return new SyncronousResult(callback, 0L);
                return _connection.BeginExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to BeginExecuteNonQuery on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        public long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            try
            {
                if (asyncResult is SyncronousResult)
                    return (long)asyncResult.AsyncState;
                else
                    return _connection.EndExecuteNonQuery(asyncResult);
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to EndExecuteNonQuery on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        public long ExecuteNonQuery(ICommand command)
        {
            if (command != null) PrepareCommand(command);
            if (!_isPrepared)
                throw new ApplicationException(
                    "You must call the PrepareCommand() method before calling ExecuteNonQuery() " +
                    "or pass the command to the ExecuteNonQuery() method");
            if (_connection == null)
                return 0L;
            try
            {
                return _connection.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to ExecuteNonQuery on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        #endregion

        #region ExecuteScalar

        public IAsyncResult BeginExecuteScalar(ICommand command, AsyncCallback callback)
        {
            if (command != null) PrepareCommand(command);
            if (!_isPrepared) 
                throw new ApplicationException(
                    "You must call the PrepareCommand() method before calling BeginExecuteScalar() "+
                    "or pass the command to the BeginExecuteScalar() method");
            try
            {
                if (_connection == null)
                    return new SyncronousResult(callback, null);
                return _connection.BeginExecuteScalar();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                    _errorReporter.ReportError(ex, "Failed to BeginExecuteScalar on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        public T EndExecuteScalar<T>(IAsyncResult asyncResult)
        {
            try
            {
                if (asyncResult is SyncronousResult)
                    return default(T);
                return _connection.EndExecuteScalar<T>(asyncResult);
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to EndExecuteScalar on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        public T ExecuteScalar<T>(ICommand command)
        {
            if (command != null) PrepareCommand(command);
            if (!_isPrepared)
                throw new ApplicationException(
                    "You must call the PrepareCommand() method before calling ExecuteScalar() " +
                    "or pass the command to the ExecuteScalar() method");
            if (_connection == null)
                return default(T);
            try
            {
                return _connection.ExecuteScalar<T>();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(_connection);
                _errorReporter.ReportError(ex, "Failed to ExecuteScalar on " + _repository.Name, _repository, _connection);
                throw;
            }
        }

        #endregion

        protected class SyncronousResult : IAsyncResult
        {
            public WaitHandle AsyncWaitHandle { get; private set; }
            public object AsyncState { get; private set; }
            public bool CompletedSynchronously { get { return true; } }
            public bool IsCompleted { get { return true; } }

            public SyncronousResult(AsyncCallback callback, object state)
            {
                AsyncState = state;
                AsyncWaitHandle = new ManualResetEvent(true);
                if (callback != null) callback(this);
            }
        }
    }
}
