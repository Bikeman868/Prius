using System;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Contracts.Utility;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    [Provider("SqLite", "SQLite database connection provider")]
    public class Connection : Disposable, IConnection, IConnectionProvider
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;
        private readonly ICommandProcessorFactory _commandProcessorFactory;
        private readonly ISchemaUpdater _schemaUpdater;

        public object RepositoryContext { get; set; }

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
                    _errorReporter.ReportError(ex, "Failed to open connection to SqLite on " + _repository.Name);
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

            _commandProcessor = _commandProcessorFactory.Create(command, _connection, _transaction);

            if (command.TimeoutSeconds.HasValue)
                _commandProcessor.CommandTimeout = command.TimeoutSeconds.Value;

            foreach (var parameter in command.GetParameters())
            {
                SQLiteParameter sqLiteParameter;
                switch (parameter.Direction)
                {
                    case ParameterDirection.Input:
                        _commandProcessor.Parameters.AddWithValue("@" + parameter.Name, parameter.Value);
                        break;
                    case ParameterDirection.InputOutput:
                        sqLiteParameter = _commandProcessor.Parameters.AddWithValue("@" + parameter.Name, parameter.Value);
                        sqLiteParameter.Direction = System.Data.ParameterDirection.InputOutput;
                        parameter.StoreOutputValue = p => p.Value = sqLiteParameter.Value;
                        break;
                    case ParameterDirection.Output:
                        sqLiteParameter = _commandProcessor.Parameters.Add("@" + parameter.Name, ToSQLiteDbType(parameter.DbType), (int)parameter.Size);
                        sqLiteParameter.Direction = System.Data.ParameterDirection.Output;
                        parameter.StoreOutputValue = p => p.Value = sqLiteParameter.Value;
                        break;
                    case ParameterDirection.ReturnValue:
                        throw new NotImplementedException("Prius does not support return values with SqLite");
                }
            }

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
                    throw new Exception("SqLite command did not return a reader");

                foreach (var parameter in _command.GetParameters())
                    parameter.StoreOutputValue(parameter);

                asyncContext.Result = reader;

            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteReader on SqLite " + _repository.Name, _repository, this);
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
                _errorReporter.ReportError(ex, "Failed to ExecuteNonQuery on SqLite " + _repository.Name, _repository, this);
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
            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == System.Data.ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed) _connection.Open();
                asyncContext.Result = _commandProcessor.ExecuteScalar();

                var elapsedTicks = PerformanceTimer.TimeNow - asyncContext.StartTime;
                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(elapsedTicks));
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                    _errorReporter.ReportError(ex, "Failed to ExecuteScalar on SqLite " + _repository.Name, _repository, this);
                throw;
            }
            finally
            {
                if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
            return new SyncronousResult(asyncContext, callback);
        }

        public T EndExecuteScalar<T>(IAsyncResult asyncResult)
        {
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            try
            {
                if (asyncContext.Result == null) return default(T);
                var resultType = typeof(T);
                if (resultType.IsNullable()) resultType = resultType.GetGenericArguments()[0];
                return (T)Convert.ChangeType(asyncContext.Result, resultType);
            }
            catch (Exception ex)
            {
                _errorReporter.ReportError(ex, "Failed to convert type of result from ExecuteScalar on SqLite  " + _repository.Name, _repository, this);
                throw;
            }
        }

        public T ExecuteScalar<T>()
        {
            return EndExecuteScalar<T>(BeginExecuteScalar(null));
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

        #region Conversions and mappings

        private System.Data.DbType ToSQLiteDbType(System.Data.SqlDbType dbType)
        {
            switch (dbType)
            {
                case System.Data.SqlDbType.BigInt:
                    return System.Data.DbType.Int64;
                case System.Data.SqlDbType.Binary:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.Bit:
                    return System.Data.DbType.Boolean;
                case System.Data.SqlDbType.Char:
                    return System.Data.DbType.Byte;
                case System.Data.SqlDbType.Date:
                    return System.Data.DbType.Date;
                case System.Data.SqlDbType.DateTime:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.DateTime2:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.DateTimeOffset:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.Decimal:
                    return System.Data.DbType.Decimal;
                case System.Data.SqlDbType.Float:
                    return System.Data.DbType.Single;
                case System.Data.SqlDbType.Image:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.Int:
                    return System.Data.DbType.UInt32;
                case System.Data.SqlDbType.Money:
                    return System.Data.DbType.Decimal;
                case System.Data.SqlDbType.NChar:
                    return System.Data.DbType.UInt32;
                case System.Data.SqlDbType.NText:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.NVarChar:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.Real:
                    return System.Data.DbType.Double;
                case System.Data.SqlDbType.SmallDateTime:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.SmallInt:
                    return System.Data.DbType.Int16;
                case System.Data.SqlDbType.SmallMoney:
                    return System.Data.DbType.Decimal;
                case System.Data.SqlDbType.Structured:
                    return System.Data.DbType.Object;
                case System.Data.SqlDbType.Text:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.Time:
                    return System.Data.DbType.Time;
                case System.Data.SqlDbType.Timestamp:
                    return System.Data.DbType.DateTimeOffset;
                case System.Data.SqlDbType.TinyInt:
                    return System.Data.DbType.Int16;
                case System.Data.SqlDbType.Udt:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.UniqueIdentifier:
                    return System.Data.DbType.Guid;
                case System.Data.SqlDbType.VarBinary:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.VarChar:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.Variant:
                    return System.Data.DbType.Object;
                case System.Data.SqlDbType.Xml:
                    return System.Data.DbType.Xml;
            }

            return System.Data.DbType.String;
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
