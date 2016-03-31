using System;
using System.Text;
using MySql.Data.MySqlClient;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.MySql
{
    public class Connection : Connections.Connection
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IDataReaderFactory _dataReaderFactory;

        private IRepository _repository;
        private ICommand _command;

        private MySqlConnection _connection;
        private MySqlTransaction _transaction;
        private MySqlCommand _mySqlCommand;

        #region Lifetime

        public Connection(
            IErrorReporter errorReporter,
            IDataEnumeratorFactory dataEnumeratorFactory, 
            IDataReaderFactory dataReaderFactory)
            : base(dataEnumeratorFactory)
        {
            _errorReporter = errorReporter;
            _dataReaderFactory = dataReaderFactory;
        }

        public IConnection Initialize(IRepository repository, ICommand command, string connectionString)
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

        public override void BeginTransaction()
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
                    _errorReporter.ReportError(ex, "Failed to open connection to MySql on " + _repository.Name);
                    throw;
                }
            }
            _transaction = _connection.BeginTransaction();
        }

        public override void Commit()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
            }
        }

        public override void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
            }
        }

        #endregion

        #region Command setup

        public override void SetCommand(ICommand command)
        {
            if (command == null) return;
            _command = command;

            _mySqlCommand = new MySqlCommand(command.CommandText, _connection, _transaction);
            _mySqlCommand.CommandType = (System.Data.CommandType)command.CommandType;

            if (command.TimeoutSeconds.HasValue)
                _mySqlCommand.CommandTimeout = command.TimeoutSeconds.Value;

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

        #region ExecuteReader

        public override IAsyncResult BeginExecuteReader(AsyncCallback callback)
        {
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
                    throw new Exception("MySQL command did not return a reader");

                foreach (var parameter in _command.GetParameters())
                    parameter.StoreOutputValue(parameter);

                var dataShapeName = _connection.DataSource + ":" + _connection.Database + ":" + _mySqlCommand.CommandType + ":" + _mySqlCommand.CommandText;
                asyncContext.Result = _dataReaderFactory.Create(
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

        public override IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            return (IDataReader)asyncContext.Result;
        }

        #endregion

        #region ExecuteNonQuery

        public override IAsyncResult BeginExecuteNonQuery(AsyncCallback callback)
        {
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

        public override long EndExecuteNonQuery(IAsyncResult asyncResult)
        {
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

        #endregion

        #region ExecuteScalar

        public override IAsyncResult BeginExecuteScalar(AsyncCallback callback)
        {
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

        public override T EndExecuteScalar<T>(IAsyncResult asyncResult)
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
                _errorReporter.ReportError(ex, "Failed to convert type of result from ExecuteScalar on MySQL Server " + _repository.Name, _repository, this);
                throw;
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
            sb.AppendFormat("CommandType='{0}'; ", _mySqlCommand.CommandType);
            sb.AppendFormat("CommandText='{0}'; ", _mySqlCommand.CommandText);
            return sb.ToString();
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

    }
}
