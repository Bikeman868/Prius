using System;
using System.Data.SqlClient;
using System.Text;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.SqlServer
{
    public class Connection : Connections.Connection
    {
        private readonly IErrorReporter _errorReporter;
        private readonly IDataReaderFactory _dataReaderFactory;

        private IRepository _repository;
        private ICommand _command;

        private SqlConnection _connection;
        private SqlTransaction _transaction;
        private SqlCommand _sqlCommand;

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
            _connection = new SqlConnection(connectionString);
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
                    _errorReporter.ReportError(ex, _sqlCommand, "Failed to open connection on " + _repository.Name);
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

            _sqlCommand = new SqlCommand(command.CommandText, _connection, _transaction);
            _sqlCommand.CommandType = (System.Data.CommandType)command.CommandType;
            if (command.TimeoutSeconds.HasValue)
                _sqlCommand.CommandTimeout = command.TimeoutSeconds.Value;
            foreach (var parameter in command.GetParameters())
            {
                var sqlParameter = _sqlCommand.Parameters.Add("@" + parameter.Name, parameter.DbType, (int)parameter.Size);
                sqlParameter.Direction = (System.Data.ParameterDirection)parameter.Direction;
                sqlParameter.Value = parameter.Value;

                if (parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Output)
                    parameter.StoreOutputValue = p => p.Value = sqlParameter.Value;
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
                return _sqlCommand.BeginExecuteReader(callback, asyncContext);
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, _sqlCommand, "Failed to ExecuteReader on SQL Server " + _repository.Name, _repository, this);
                if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
                throw;
            }
        }

        public override IDataReader EndExecuteReader(IAsyncResult asyncResult)
        {
            var asyncContext = (AsyncContext)asyncResult.AsyncState;
            try
            {
                if (asyncContext.Result != null) return (IDataReader)asyncContext.Result;

                var reader = _sqlCommand.EndExecuteReader(asyncResult);

                foreach (var parameter in _command.GetParameters())
                    parameter.StoreOutputValue(parameter);

                var dataShapeName = _connection.DataSource + ":" + _connection.Database + ":" + _sqlCommand.CommandType + ":" + _sqlCommand.CommandText;

                return _dataReaderFactory.Create(
                    reader,
                    dataShapeName,
                    () =>
                    {
                        reader.Dispose();
                        _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));
                        if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open)
                            _connection.Close();
                    },
                    () =>
                    {
                        _repository.RecordFailure(this);
                        if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open)
                            _connection.Close();
                    }
                    );
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, _sqlCommand, "Failed to ExecuteReader on SQL Server " + _repository.Name, _repository, this);
                if (asyncContext.InitiallyClosed && _connection.State == System.Data.ConnectionState.Open)
                    _connection.Close();
                throw;
            }
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
                return _sqlCommand.BeginExecuteNonQuery(callback, asyncContext);
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, _sqlCommand, "Failed to ExecuteNonQuery on SQL Server " + _repository.Name, _repository, this);
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

                var rowsAffacted = _sqlCommand.EndExecuteNonQuery(asyncResult);
                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));

                foreach (var parameter in _command.GetParameters())
                    parameter.StoreOutputValue(parameter);

                return rowsAffacted;
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, _sqlCommand, "Failed to ExecuteNonQuery on SQL Server " + _repository.Name, _repository, this);
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
                asyncContext.Result = _sqlCommand.ExecuteScalar();
                _repository.RecordSuccess(this, PerformanceTimer.TicksToSeconds(PerformanceTimer.TimeNow - asyncContext.StartTime));
            }
            catch (Exception ex)
            {
                _repository.RecordFailure(this);
                _errorReporter.ReportError(ex, _sqlCommand, "Failed to ExecuteScalar on SQL Server " + _repository.Name, _repository, this);
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
                _errorReporter.ReportError(ex, _sqlCommand, "Failed to convert type of result from ExecuteScalar on SQL Server " + _repository.Name, _repository, this);
                throw;
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            var sb = new StringBuilder("SQL Server connection: ");
            sb.AppendFormat("Repository='{0}'; ", _repository.Name);
            sb.AppendFormat("Database='{0}'; ", _connection.Database);
            sb.AppendFormat("DataSource='{0}'; ", _connection.DataSource);
            sb.AppendFormat("CommandType='{0}'; ", _sqlCommand.CommandType);
            sb.AppendFormat("CommandText='{0}'; ", _sqlCommand.CommandText);
            return sb.ToString();
        }

        #endregion

    }
}
