using System;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using Prius.Contracts.Attributes;
using Prius.Contracts.Exceptions;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Contracts.Utility;
using Prius.SQLite.Interfaces;

namespace Prius.SQLite.CommandProcessing
{
    /// <summary>
    /// This is the main entry point into the SQLite driver from Prius.
    /// It defines that any Prius database using a server type 'SQLite'
    /// will be handled by this connection provider.
    /// </summary>
    [Provider("SQLite", "SQLite database connection provider")]
    public class Connection : Disposable, IConnection, IConnectionProvider
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;
        private readonly ICommandProcessorFactory _commandProcessorFactory;
        private readonly ISchemaUpdater _schemaUpdater;

        public object RepositoryContext { get; set; }
        public ITraceWriter TraceWriter { get; set; }

        private IRepository _repository;
        private ICommand _command;
        private string _dataShapeName;

        private SQLiteConnection _connection;
        private SQLiteTransaction _transaction;
        private ICommandProcessor _commandProcessor;

        #region Lifetime

        public Connection(
            IErrorReporter errorReporter,
            IDataEnumeratorFactory dataEnumeratorFactory, 
            ICommandProcessorFactory commandProcessorFactory, 
            ISchemaUpdater schemaUpdater)
        {
            _errorReporter = errorReporter;
            _dataEnumeratorFactory = dataEnumeratorFactory;
            _commandProcessorFactory = commandProcessorFactory;
            _schemaUpdater = schemaUpdater;
        }

        public IConnection Open(IRepository repository, ICommand command, string connectionString, string schemaName)
        {
            _repository = repository;
            _connection = new SQLiteConnection(connectionString);
            _transaction = null;

            _schemaUpdater.CheckSchema(_repository, _connection);

            SetCommand(command);
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            Commit();
            _commandProcessor.Dispose();
            _connection.Dispose();
            base.Dispose(destructor);
        }
        
        #endregion

        #region Transactions

        public void BeginTransaction()
        {
            Commit();
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    _connection.Open();
                }
                catch (Exception ex)
                {
                    _repository.RecordFailure(this);
                    _errorReporter.ReportError(ex, "Failed to open connection to SQLite on " + _repository.Name);
                    throw;
                }
            }
            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
            }
        }

        #endregion

        #region Command setup

        public void SetCommand(ICommand command)
        {
            if (command == null) return;
            _command = command;

            _commandProcessor = _commandProcessorFactory.CreateAdo(_repository, command, _connection, _transaction);

            _dataShapeName = _connection.DataSource + ":" + _connection.Database + ":" + command.CommandType + ":" + command.CommandText;
        }

        #endregion

        #region ExecuteEnumerable

        public IAsyncResult BeginExecuteEnumerable(AsyncCallback callback)
        {
            return BeginExecuteReader(callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class
        {
            var reader = EndExecuteReader(asyncResult);
            return _dataEnumeratorFactory.Create<T>(reader, reader.Dispose);
        }

        public IDataEnumerator<T> ExecuteEnumerable<T>() where T : class
        {
            var reader = ExecuteReader();
            return _dataEnumeratorFactory.Create<T>(reader, reader.Dispose);
        }

        #endregion

        #region ExecuteReader

        public IAsyncResult BeginExecuteReader(AsyncCallback callback)
        {
            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == System.Data.ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed) _connection.Open();
                var reader = _commandProcessor.ExecuteReader(
                    _dataShapeName,
                    r =>
                        {
                            var elapsedTicks = PerformanceTimer.TimeNow - asyncContext.StartTime;
                            _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(elapsedTicks));

                            if (asyncContext.InitiallyClosed) 
                                _connection.Close();
                        },
                    r =>
                        {
                            _repository.RecordFailure(this);

                            if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open) 
                                _connection.Close();
                        }
                    );

                if (reader == null)
                    throw new PriusException("SQLite command did not return a reader");

                foreach (var parameter in _command.GetParameters())
                    parameter.StoreOutputValue(parameter);

                asyncContext.Result = reader;

            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteReader on SQLite " + _repository.Name, _repository, this);
                if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open) _connection.Close();
                throw;
            }
            return new SyncronousResult(asyncContext, callback);
        }

        public IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            return (IDataReader)asyncContext.Result;
        }

        public virtual IDataReader ExecuteReader()
        {
            return EndExecuteReader(BeginExecuteReader(null));
        }

        #endregion

        #region ExecuteNonQuery

        public IAsyncResult BeginExecuteNonQuery(AsyncCallback callback)
        {
            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == System.Data.ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed) _connection.Open();
                asyncContext.Result = _commandProcessor.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteNonQuery on SQLite " + _repository.Name, _repository, this);
                asyncContext.Result = (long)0;
                throw;
            }
            return new SyncronousResult(asyncContext, callback);
        }

        public long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            return (long)asyncContext.Result;
        }

        public long ExecuteNonQuery()
        {
            return EndExecuteNonQuery(BeginExecuteNonQuery(null));
        }

        #endregion

        #region ExecuteScalar

        public IAsyncResult BeginExecuteScalar(AsyncCallback callback)
        {
            return new SyncronousResult(new AsyncContext(), callback);
        }

        public T EndExecuteScalar<T>(IAsyncResult asyncResult)
        {
            return ExecuteScalar<T>();
        }

        public T ExecuteScalar<T>()
        {
            var initiallyClosed = _connection.State == System.Data.ConnectionState.Closed;

            try
            {
                var startTime = PerformanceTimer.TimeNow;

                if (initiallyClosed) _connection.Open();
                var result = _commandProcessor.ExecuteScalar<T>();

                var elapsedTicks = PerformanceTimer.TimeNow - startTime;
                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(elapsedTicks));

                return result;
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteScalar on SQLite repository '" + _repository.Name + "'", _repository, this);
                throw;
            }
            finally
            {
                if (initiallyClosed && _connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            var sb = new StringBuilder("MySQL connection: ");
            sb.AppendFormat("Repository='{0}'; ", _repository.Name);
            sb.AppendFormat("Database='{0}'; ", _connection.Database);
            sb.AppendFormat("DataSource='{0}'; ", _connection.DataSource);
            sb.AppendFormat("Command='{0}'; ", _commandProcessor);
            return sb.ToString();
        }

        #endregion

        #region private classes

        private class AsyncContext
        {
            public object Result;
            public bool InitiallyClosed;
            public long StartTime;
        }

        private class SyncronousResult : IAsyncResult
        {
            public WaitHandle AsyncWaitHandle { get; private set; }
            public object AsyncState { get; private set; }
            public bool CompletedSynchronously { get { return true; } }
            public bool IsCompleted { get { return true; } }

            public SyncronousResult(AsyncContext asyncContext, AsyncCallback callback)
            {
                AsyncState = asyncContext;
                AsyncWaitHandle = new ManualResetEvent(true);
                if (callback != null) callback(this);
            }
        }

        #endregion
    }
}
