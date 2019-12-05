using System;
using System.Text;
using System.Threading;
using Npgsql;
using NpgsqlTypes;
using Prius.Contracts.Exceptions;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Contracts.Utility;

namespace Prius.PostgreSql
{
    public class Connection : Disposable, IConnection, IConnectionProvider
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;

        public object RepositoryContext { get; set; }
        public ITraceWriter TraceWriter { get; set; }
        public IAnalyticRecorder AnalyticRecorder { get; set; }

        private IRepository _repository;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        private NpgsqlCommand _command;
        private bool _isBulkCopy;
        private string _schema;
        private System.Data.DataTable _dataTable;

        private const string _validEscapes = "bfnrtv";

        #region Lifetime

        public Connection(
            IErrorReporter errorReporter,
            IDataEnumeratorFactory dataEnumeratorFactory)
        {
            _errorReporter = errorReporter;
            _dataEnumeratorFactory = dataEnumeratorFactory;
        }

        public IConnection Open(
            IRepository repository, 
            ICommand command, 
            string connectionString, 
            string schema,
            ITraceWriter traceWriter,
            IAnalyticRecorder analyticRecorder)
        {
            _repository = repository;
            _connection = new NpgsqlConnection(connectionString);

#if DEBUG
            NpgsqlEventLog.Level = LogLevel.Debug;
            NpgsqlEventLog.EchoMessages = true;
#endif
      
            _transaction = null;
            _schema=schema;
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
                    _errorReporter.ReportError(ex, "Failed to open connection to Postgesql on " + _repository.Name);
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
            _command = new NpgsqlCommand(
                string.Format("{0}.{1}", _schema,command.CommandText), 
                _connection,
                _transaction)
                {
                    CommandType = (System.Data.CommandType) command.CommandType,
                };

            if (command.TimeoutSeconds.HasValue)
                _command.CommandTimeout = command.TimeoutSeconds.Value;

            foreach (var parameter in command.GetParameters())
            {
                if (parameter.DbType == System.Data.SqlDbType.Structured)
                {
                    _command.CommandType = System.Data.CommandType.Text;
                    _isBulkCopy = true;
                    _dataTable = (System.Data.DataTable)parameter.Value;
                    break;
                }
                else
                {
                    _isBulkCopy = false;
                    var sqlParameter = _command.Parameters.Add("@" + parameter.Name, NpgsqlDbTypeFrom(parameter.DbType), (int)parameter.Size);
                    sqlParameter.Direction = (System.Data.ParameterDirection)parameter.Direction;
                    sqlParameter.Value = parameter.Value;
                }
            }
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
                var reader = _command.ExecuteReader();
                var dataShapeName = _connection.DataSource + ":" + _connection.Database + ":" + _command.CommandType + ":" + _command.CommandText;
                asyncContext.Result = new DataReader(_errorReporter).Initialize(
                    reader,
                    dataShapeName,
                    () =>
                    {
                        reader.Dispose();
                        _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));
                        if (asyncContext.InitiallyClosed) _connection.Close();
                    },
                    () =>
                    {
                        _repository.RecordFailure(this);
                        if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open) _connection.Close();
                    }
                    );
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteReader on PostgrSql " + _repository.Name, _repository, this);
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

                if (_isBulkCopy && _dataTable != null)
                {
                    BulkCopy(_dataTable, _connection, _command);
                    asyncContext.Result = (long)_dataTable.Rows.Count;
                }
                else
                {
                    asyncContext.Result = (long)_command.ExecuteNonQuery();
                }

                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));
                if (asyncContext.InitiallyClosed) _connection.Close();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, string.Format("Failed to ExecuteNonQuery on PostgreSql {0}\nConnection State: {1}", _repository.Name, _connection.FullState), _repository, this);
                asyncContext.Result = 0L;
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
                asyncContext.Result = _command.ExecuteScalar();
                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));
                if (asyncContext.InitiallyClosed) _connection.Close();
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteScalar on PostgrSql " + _repository.Name, _repository, this);
                throw;
            }
            return new SyncronousResult(asyncContext, callback);
        }

        public T EndExecuteScalar<T>(IAsyncResult asyncResult)
        {
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            try
            {
                if (asyncContext.Result == null) return default(T);
                return (T)Convert.ChangeType(asyncContext.Result, typeof(T));
            }
            catch (Exception ex)
            {
                _errorReporter.ReportError(ex, "Failed to convert type of result from ExecuteScalar on PostgrSql " + _repository.Name, _repository, this);
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
            var sb = new StringBuilder("PostgreSql connection: ");
            sb.AppendFormat("Repository='{0}'; ", _repository.Name);
            sb.AppendFormat("Database='{0}'; ", _connection.Database);
            sb.AppendFormat("DataSource='{0}'; ", _connection.DataSource);
            sb.AppendFormat("CommandType='{0}'; ", _command.CommandType);
            sb.AppendFormat("CommandText='{0}'; ", _command.CommandText);
            return sb.ToString();
        }

        #endregion

        #region Helper methods

        private void BulkCopy(System.Data.DataTable items, NpgsqlConnection connection, NpgsqlCommand command)
        {
            var dataTableString = DataTableToString(items);

            var sql = string.Format("COPY {0} (\"{1}\") FROM STDIN WITH DELIMITER '|'", command.CommandText, dataTableString);

            command.CommandText = sql;

            var copy = new NpgsqlCopyIn(command, connection);
            try
            {
                copy.Start();
                foreach (System.Data.DataRow item in items.Rows)
                {
                    var data = SerializeData(item.ItemArray);
                    var raw = Encoding.UTF8.GetBytes(string.Concat(data, "\n"));
                    copy.CopyStream.Write(raw, 0, raw.Length);
                }
            }
            catch (Exception e)
            {
                try
                {
                    copy.Cancel("Undo copy");
                }
                catch (NpgsqlException e2)
                {
                    // we should get an error in response to our cancel request:
                    if (!("" + e2).Contains("Undo copy"))
                    {
                        throw new PriusException("Failed to cancel copy: " + copy + " upon failure: " + e);
                    }
                }

                throw;
            }
            finally
            {
                copy.CopyStream?.Close();

                copy.End();
            }  
        }

        private string DataTableToString(System.Data.DataTable items)
        {
            string result;
            var sb = new StringBuilder();
            try
            {
                bool first = true;
                foreach (System.Data.DataColumn column in items.Columns)
                {
                    if (first)
                        first = false;
                    else
                        sb.Append(",");
                    sb.Append("\"");
                    sb.Append(column.ColumnName);
                    sb.Append("\"");
                }
                result = sb.ToString().Trim('\"');
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }

        private string SerializeData(object[] data)
        {
            string resultString;
            var sb = new StringBuilder();
            foreach (var d in data)
            {
                if (sb.Length > 0)
                    sb.Append("|"); 
                if (d == null || d is System.DBNull)
                {
                    sb.Append("\\N");
                }
                else if (d is DateTime)
                {
                    sb.Append(((DateTime) d).ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else if (d is Enum)
                {
                    sb.Append(((Enum) d).ToString("d"));
                }
                else
                {
                    sb.Append(SerializeData(d));
                }
                   
            }
            resultString = sb.ToString();
            return resultString;
        }

        private string SerializeData(object data)
        {
            var unprocessedString = data.ToString();
            string processedString;
            var sb = new StringBuilder();
            for (int index = 0; index < unprocessedString.Length; index++)
            {
                char c = unprocessedString[index];
                // escape column sep, escape char, new line characters (http://www.postgresql.org/docs/9.2/static/sql-copy.html)
                if (c == '|' || c == '\\' || c == '\n' || c == '\r')
                    sb.Append('\\');
                sb.Append(c);
                if (c == '\\')
                {
                    c = unprocessedString[++index];
                    if (_validEscapes.IndexOf(c) < 0)
                    {
                        sb.Append('\\');
                    }
                    sb.Append(c);
                }
            }
            processedString = sb.ToString();
            return processedString;
        }

        public static NpgsqlDbType NpgsqlDbTypeFrom(System.Data.SqlDbType dbType)
        {
            if (dbType == System.Data.SqlDbType.Int) return NpgsqlDbType.Integer;
            if (dbType == System.Data.SqlDbType.BigInt) return NpgsqlDbType.Bigint;
            if (dbType == System.Data.SqlDbType.VarChar) return NpgsqlDbType.Varchar;
            if (dbType == System.Data.SqlDbType.UniqueIdentifier) return NpgsqlDbType.Uuid;
            if (dbType == System.Data.SqlDbType.Float) return NpgsqlDbType.Double;
            if (dbType == System.Data.SqlDbType.DateTime) return NpgsqlDbType.Timestamp;
            if (dbType == System.Data.SqlDbType.Binary) return NpgsqlDbType.Bytea;
            //if (dbType == System.Data.SqlDbType.Structured) return NpgsqlDbType.;
            return NpgsqlDbType.Varchar;
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
