using System;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using Prius.Contracts.Attributes;
using Prius.Contracts.Exceptions;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Contracts.Utility;
using Prius.SQLite.Interfaces;
using IDataReader = Prius.Contracts.Interfaces.IDataReader;

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

        private const string ServerType = "SQLite";

        private static volatile int _activeCount;

        public object RepositoryContext { get; set; }
        public ITraceWriter TraceWriter { get; set; }
        public IAnalyticRecorder AnalyticRecorder { get; set; }

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
            _activeCount++;
        }

        public IConnection Open(
            IRepository repository, 
            ICommand command, 
            string connectionString, 
            string schemaName,
            ITraceWriter traceWriter,
            IAnalyticRecorder analyticRecorder)
        {
            _repository = repository;
            _connection = new SQLiteConnection(connectionString);
            _transaction = null;

            TraceWriter = traceWriter;
            AnalyticRecorder = analyticRecorder;

            _schemaUpdater.CheckSchema(_repository, _connection);

            SetCommand(command);

            return this;
        }

        protected override void Dispose(bool destructor)
        {
            try
            {
                Commit();
                CloseConnection();

                _commandProcessor.Dispose();
                _connection.Dispose();
                base.Dispose(destructor);
            }
            finally
            {
                _activeCount--;
            }
        }

        private void OpenConnection()
        {
            try
            {
                Trace("Opening a connection to SQlite database");

                _connection.Open();

                AnalyticRecorder?.ConnectionOpened(new ConnectionAnalyticInfo
                    {
                        ServerType = ServerType,
                        RepositoryName = _repository.Name,
                        ConnectionString = _connection.ConnectionString,
                        ActiveCount = _activeCount
                    });
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);

                _errorReporter.ReportError(ex, "Failed to open connection to SQLite on " + _repository.Name);

                AnalyticRecorder?.ConnectionFailed(new ConnectionAnalyticInfo
                {
                    ServerType = ServerType,
                    RepositoryName = _repository.Name,
                    ConnectionString = _connection.ConnectionString,
                    ActiveCount = _activeCount
                });

                throw;
            }
        }

        private void CloseConnection()
        {
            if (_connection.State == ConnectionState.Closed)
                return;

            try
            {
                Trace("Closing the connection to SQlite database");
                _connection.Close();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to close connection to SQLite on " + _repository.Name);
                throw;
            }
            finally
            {
                AnalyticRecorder?.ConnectionClosed(new ConnectionAnalyticInfo
                {
                    ServerType = ServerType,
                    RepositoryName = _repository.Name,
                    ConnectionString = _connection.ConnectionString,
                    ActiveCount = _activeCount
                });
            }
        }

        #endregion

        #region Transactions

        public void BeginTransaction()
        {
            Commit();

            if (_connection.State == ConnectionState.Closed)
                OpenConnection();

            Trace("Starting a new SQlite transaction");

            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            if (_transaction != null)
            {
                Trace("Committing SQlite transaction");
                _transaction.Commit();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                Trace("Rolling back SQlite transaction");
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
            Trace("SQlite begin execute enumerable");
            return BeginExecuteReader(callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class
        {
            Trace("SQlite end execute enumerable");
            var reader = EndExecuteReader(asyncResult);
            return _dataEnumeratorFactory.Create<T>(reader, reader.Dispose);
        }

        public IDataEnumerator<T> ExecuteEnumerable<T>() where T : class
        {
            Trace("SQlite execute enumerable");
            var reader = ExecuteReader();
            return _dataEnumeratorFactory.Create<T>(reader, reader.Dispose);
        }

        #endregion

        #region ExecuteReader

        public IAsyncResult BeginExecuteReader(AsyncCallback callback)
        {
            Trace("SQlite begin execute reader");
            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed)
                    OpenConnection();

                var reader = _commandProcessor.ExecuteReader(
                    _dataShapeName,
                    r =>
                        {
                            var elapsedSeconds = PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime);
                            _repository.RecordSuccess(this, elapsedSeconds);
                            AnalyticRecorder?.CommandCompleted(new CommandAnalyticInfo
                            {
                                Connection = this,
                                Command = _command,
                                ElapsedSeconds = elapsedSeconds
                            });

                            if (asyncContext.InitiallyClosed) 
                                CloseConnection();
                        },
                    r =>
                        {
                            _repository.RecordFailure(this);
                            AnalyticRecorder?.CommandFailed(new CommandAnalyticInfo
                            {
                                Connection = this,
                                Command = _command,
                            });

                            if (asyncContext.InitiallyClosed)
                                CloseConnection();
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
                Trace("SQlite exception executing reader");
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteReader on SQLite " + _repository.Name, _repository, this);

                if (asyncContext.InitiallyClosed)
                    CloseConnection();

                throw;
            }
            return new SyncronousResult(asyncContext, callback);
        }

        public IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            Trace("SQlite end execute reader");
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            var elapsedSeconds = PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime);
            _repository.RecordSuccess(this, elapsedSeconds);
            AnalyticRecorder?.CommandCompleted(new CommandAnalyticInfo
            {
                Connection = this,
                Command = _command,
                ElapsedSeconds = elapsedSeconds
            });
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
            Trace("SQlite begin execute non-query");
            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed)
                    OpenConnection();

                asyncContext.Result = _commandProcessor.ExecuteNonQuery();

                var elapsedSeconds = PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime);
                _repository.RecordSuccess(this, elapsedSeconds);
                AnalyticRecorder?.CommandCompleted(new CommandAnalyticInfo
                {
                    Connection = this,
                    Command = _command,
                    ElapsedSeconds = elapsedSeconds
                });
            }
            catch (Exception ex)
            {
                Trace("Exception executing SQlite non-query");
                _repository.RecordFailure(this);
                AnalyticRecorder?.CommandFailed(new CommandAnalyticInfo
                {
                    Connection = this,
                    Command = _command,
                });
                _errorReporter.ReportError(ex, "Failed to ExecuteNonQuery on SQLite " + _repository.Name, _repository, this);
                asyncContext.Result = (long)0;
                throw;
            }
            return new SyncronousResult(asyncContext, callback);
        }

        public long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            Trace("SQlite end execute non-query");
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
            Trace("SQlite begin execute scalar");
            return new SyncronousResult(new AsyncContext(), callback);
        }

        public T EndExecuteScalar<T>(IAsyncResult asyncResult)
        {
            Trace("SQlite end execute scalar");
            return ExecuteScalar<T>();
        }

        public T ExecuteScalar<T>()
        {
            Trace("SQlite execute scalar");
            var initiallyClosed = _connection.State == ConnectionState.Closed;

            try
            {
                var startTime = PerformanceTimer.TimeNow;

                if (initiallyClosed)
                    OpenConnection();

                var result = _commandProcessor.ExecuteScalar<T>();

                var elapsedSeconds = PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - startTime);
                _repository.RecordSuccess(this, elapsedSeconds);
                AnalyticRecorder?.CommandCompleted(new CommandAnalyticInfo
                {
                    Connection = this,
                    Command = _command,
                    ElapsedSeconds = elapsedSeconds
                });

                return result;
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                AnalyticRecorder?.CommandFailed(new CommandAnalyticInfo
                {
                    Connection = this,
                    Command = _command,
                });
                _errorReporter.ReportError(ex, "Failed to ExecuteScalar on SQLite repository '" + _repository.Name + "'", _repository, this);
                throw;
            }
            finally
            {
                if (initiallyClosed)
                    CloseConnection();
            }
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            var sb = new StringBuilder("MySQL connection: ");
            sb.AppendFormat("Repository='{0}'; ", _repository.Name);
            sb.AppendFormat("Database='{0}'; ", _connection.Database);
            sb.AppendFormat("DataSource='{0}'; ", _connection.DataSource);
            sb.AppendFormat("Command='{0}'; ", _commandProcessor);
            return sb.ToString();
        }

        private void Trace(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            TraceWriter?.WriteLine(message);
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
                callback?.Invoke(this);
            }
        }

        #endregion
    }
}
