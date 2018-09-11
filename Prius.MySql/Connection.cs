using System;
using System.Linq;
using System.Text;
using System.Threading;
using MySql.Data.MySqlClient;
using Prius.Contracts.Attributes;
using Prius.Contracts.Exceptions;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Contracts.Utility;

namespace Prius.MySql
{
    [Provider("MySql", "MySQL, Aurora and MariaDB connection provider")]
    public class Connection : Disposable, IConnection, IConnectionProvider
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;

        public object RepositoryContext { get; set; }
        public ITraceWriter TraceWriter { get; set; }

        private IRepository _repository;
        private ICommand _command;

        private MySqlConnection _connection;
        private MySqlTransaction _transaction;
        private MySqlCommand _mySqlCommand;

        #region Lifetime

        public Connection(
            IErrorReporter errorReporter,
            IDataEnumeratorFactory dataEnumeratorFactory)
        {
            _errorReporter = errorReporter;
            _dataEnumeratorFactory = dataEnumeratorFactory;
        }

        public IConnection Open(IRepository repository, ICommand command, string connectionString, string schemaName)
        {
            _repository = repository;
            _connection = new MySqlConnection(connectionString);
            _transaction = null;
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
                    Trace("Opening connection to MySQL database");
                    _connection.Open();
                }
                catch (Exception ex)
                {
                    _repository.RecordFailure(this);
                    _errorReporter.ReportError(ex, "Failed to open connection to MySql on " + _repository.Name);
                    throw;
                }
            }
            Trace("Starting a new MySQL transaction");
            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            if (_transaction != null)
            {
                Trace("Committing MySQL transaction");
                _transaction.Commit();
                _transaction = null;
            }
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                Trace("Rolling back MySQL transaction");
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

            Trace("Creating MySQL command " + command.CommandText);

            _mySqlCommand = new MySqlCommand(command.CommandText, _connection, _transaction);
            _mySqlCommand.CommandType = (System.Data.CommandType)command.CommandType;

            if (command.TimeoutSeconds.HasValue)
                _mySqlCommand.CommandTimeout = command.TimeoutSeconds.Value;

            Trace("Binding parameters to MySQL command");

            foreach (var parameter in command.GetParameters())
            {
                MySqlParameter mySqlParameter;
                switch (parameter.Direction)
                {
                    case Contracts.Attributes.ParameterDirection.Input:
                        _mySqlCommand.Parameters.AddWithValue("@" + parameter.Name, parameter.Value);
                        break;
                    case Contracts.Attributes.ParameterDirection.InputOutput:
                        mySqlParameter = _mySqlCommand.Parameters.AddWithValue("@" + parameter.Name, parameter.Value);
                        mySqlParameter.Direction = System.Data.ParameterDirection.InputOutput;
                        parameter.StoreOutputValue = p => p.Value = mySqlParameter.Value;
                        break;
                    case Contracts.Attributes.ParameterDirection.Output:
                        mySqlParameter = _mySqlCommand.Parameters.Add("@" + parameter.Name, ToMySqlDbType(parameter.DbType), (int)parameter.Size);
                        mySqlParameter.Direction = System.Data.ParameterDirection.Output;
                        parameter.StoreOutputValue = p => p.Value = mySqlParameter.Value;
                        break;
                    case Contracts.Attributes.ParameterDirection.ReturnValue:
                        throw new NotImplementedException("Prius does not support return values with MySQL");
                }
            }
        }

        #endregion

        #region ExecuteEnumerable

        public IAsyncResult BeginExecuteEnumerable(AsyncCallback callback)
        {
            Trace("MySQL begin execute enumerable");
            return BeginExecuteReader(callback);
        }

        public IDataEnumerator<T> EndExecuteEnumerable<T>(IAsyncResult asyncResult) where T : class
        {
            Trace("MySQL end execute enumerable");
            var reader = EndExecuteReader(asyncResult);
            return _dataEnumeratorFactory.Create<T>(reader, reader.Dispose);
        }

        public IDataEnumerator<T> ExecuteEnumerable<T>() where T : class
        {
            Trace("MySQL execute enumerable");
            var reader = ExecuteReader();
            return _dataEnumeratorFactory.Create<T>(reader, reader.Dispose);
        }

        #endregion

        #region ExecuteReader

        public IAsyncResult BeginExecuteReader(AsyncCallback callback)
        {
            Trace("Begin MySQL execute reader");

            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == System.Data.ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed) _connection.Open();
                var reader = _mySqlCommand.ExecuteReader();
                if (reader == null)
                    throw new PriusException("MySQL command did not return a reader", null, null, this, _repository);

                foreach (var parameter in _command.GetParameters())
                    parameter.StoreOutputValue(parameter);

                var dataShapeName = _connection.DataSource + ":" + _connection.Database + ":" + _mySqlCommand.CommandType + ":" + _mySqlCommand.CommandText;
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
                _errorReporter.ReportError(ex, "Failed to ExecuteReader on MySQL " + _repository.Name, _repository, this);
                if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open) _connection.Close();
                throw;
            }
            return new SyncronousResult(asyncContext, callback);
        }

        public IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            Trace("End MySQL execute reader");

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
            Trace("Begin MySQL execute non query");
            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == System.Data.ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed) _connection.Open();
                return _mySqlCommand.BeginExecuteNonQuery(callback, asyncContext);
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, "Failed to ExecuteNonQuery on MySQL Server " + _repository.Name, _repository, this);
                asyncContext.Result = (long)0;
                throw;
            }
        }

        public long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
            Trace("End MySQL execute non query");
            
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            try
            {
                if (asyncContext.Result != null) return (long)asyncContext.Result;

                var rowsAffacted = _mySqlCommand.EndExecuteNonQuery(asyncResult);
                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));

                foreach (var parameter in _command.GetParameters())
                    parameter.StoreOutputValue(parameter);

                return rowsAffacted;
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                    _errorReporter.ReportError(ex, "Failed to ExecuteNonQuery on MySQL Server " + _repository.Name, _repository, this);
                throw;
            }
            finally
            {
                if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
            }
        }

        public long ExecuteNonQuery()
        {
            return EndExecuteNonQuery(BeginExecuteNonQuery(null));
        }

        #endregion

        #region ExecuteScalar

        public IAsyncResult BeginExecuteScalar(AsyncCallback callback)
        {
            Trace("Begin MySQL execute scalar");

            var asyncContext = new AsyncContext
            {
                InitiallyClosed = _connection.State == System.Data.ConnectionState.Closed,
                StartTime = PerformanceTimer.TimeNow
            };

            try
            {
                if (asyncContext.InitiallyClosed) _connection.Open();
                asyncContext.Result = _mySqlCommand.ExecuteScalar();
                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                    _errorReporter.ReportError(ex, "Failed to ExecuteScalar on MySQL Server " + _repository.Name, _repository, this);
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
            Trace("End MySQL execute scalar");

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
                _errorReporter.ReportError(ex, "Failed to convert type of result from ExecuteScalar on MySQL Server " + _repository.Name, _repository, this);
                throw;
            }
        }

        public T ExecuteScalar<T>()
        {
            return EndExecuteScalar<T>(BeginExecuteScalar(null));
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            var sb = new StringBuilder("MySQL connection: ");
            sb.AppendFormat("Repository='{0}'; ", _repository.Name);
            sb.AppendFormat("Database='{0}'; ", _connection.Database);
            sb.AppendFormat("DataSource='{0}'; ", _connection.DataSource);
            sb.AppendFormat("CommandType='{0}'; ", _mySqlCommand.CommandType);
            sb.AppendFormat("CommandText='{0}'; ", _mySqlCommand.CommandText);
            return sb.ToString();
        }

        private void Trace(string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            var traceWriter = TraceWriter;
            if (traceWriter != null)
                traceWriter.WriteLine(message);
        }

        #endregion

        #region Conversions and mappings

        private MySqlDbType ToMySqlDbType(System.Data.SqlDbType dbType)
        {
            switch (dbType)
            {
                case System.Data.SqlDbType.BigInt:
                    return MySqlDbType.Int64;
                case System.Data.SqlDbType.Binary:
                    return MySqlDbType.Binary;
                case System.Data.SqlDbType.Bit:
                    return MySqlDbType.Bit;
                case System.Data.SqlDbType.Char:
                    return MySqlDbType.UByte;
                case System.Data.SqlDbType.Date:
                    return MySqlDbType.Date;
                case System.Data.SqlDbType.DateTime:
                    return MySqlDbType.DateTime;
                case System.Data.SqlDbType.DateTime2:
                    return MySqlDbType.DateTime;
                case System.Data.SqlDbType.DateTimeOffset:
                    return MySqlDbType.DateTime;
                case System.Data.SqlDbType.Decimal:
                    return MySqlDbType.Decimal;
                case System.Data.SqlDbType.Float:
                    return MySqlDbType.Float;
                case System.Data.SqlDbType.Image:
                    return MySqlDbType.VarBinary;
                case System.Data.SqlDbType.Int:
                    return MySqlDbType.UInt32;
                case System.Data.SqlDbType.Money:
                    return MySqlDbType.Decimal;
                case System.Data.SqlDbType.NChar:
                    return MySqlDbType.UInt32;
                case System.Data.SqlDbType.NText:
                    return MySqlDbType.LongText;
                case System.Data.SqlDbType.NVarChar:
                    return MySqlDbType.VarChar;
                case System.Data.SqlDbType.Real:
                    return MySqlDbType.Double;
                case System.Data.SqlDbType.SmallDateTime:
                    return MySqlDbType.DateTime;
                case System.Data.SqlDbType.SmallInt:
                    return MySqlDbType.Int16;
                case System.Data.SqlDbType.SmallMoney:
                    return MySqlDbType.Decimal;
                case System.Data.SqlDbType.Structured:
                    return MySqlDbType.Set;
                case System.Data.SqlDbType.Text:
                    return MySqlDbType.Binary;
                case System.Data.SqlDbType.Time:
                    return MySqlDbType.Text;
                case System.Data.SqlDbType.Timestamp:
                    return MySqlDbType.Timestamp;
                case System.Data.SqlDbType.TinyInt:
                    return MySqlDbType.Int16;
                case System.Data.SqlDbType.Udt:
                    return MySqlDbType.String;
                case System.Data.SqlDbType.UniqueIdentifier:
                    return MySqlDbType.Guid;
                case System.Data.SqlDbType.VarBinary:
                    return MySqlDbType.VarBinary;
                case System.Data.SqlDbType.VarChar:
                    return MySqlDbType.VarChar;
                case System.Data.SqlDbType.Variant:
                    return MySqlDbType.Blob;
                case System.Data.SqlDbType.Xml:
                    return MySqlDbType.LongText;
            }

            return MySqlDbType.VarString;
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
