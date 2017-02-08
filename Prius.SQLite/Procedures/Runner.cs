using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.Procedures
{
    /// <summary>
    /// Implements IProcedureRunner by detecting the type of stored procedure
    /// and constructing an execution context for the procedure before running it.
    /// When the timeout value is 0 runs the stored procedure in the current thread.
    /// When timeout is not 0 spawns a task to run the stored procedure so that the
    /// stored procedure can be aborted if it exceeds the timeout length.
    /// </summary>
    internal class Runner : IProcedureRunner
    {
        public IDataReader ExecuteReader(
            IProcedure procedure, 
            ICommand command, 
            int timeoutSeconds, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction, 
            string dataShapeName, 
            Action<IDataReader> closeAction, 
            Action<IDataReader> errorAction)
        {
            if (procedure == null) return null;

            var adoProcedure = procedure as IAdoProcedure;
            if (adoProcedure != null)
            {
                if (timeoutSeconds == 0)
                {
                    return ExecuteReader(adoProcedure, command, connection,
                        transaction, dataShapeName, closeAction, errorAction);
                }

                return ExecuteReader(adoProcedure, command, timeoutSeconds, connection,
                    transaction, dataShapeName, closeAction, errorAction);
            }

            var nativeProcedure = procedure as INativeProcedure;
            if (nativeProcedure != null)
            {
                throw new NotImplementedException("Procedures that access the SqLite engine natively are not supported in this version");
            }

            throw new Exception("Unknown procedure type '" + procedure.GetType().FullName + "'");
        }

        public T ExecuteScalar<T>(
            IProcedure procedure, 
            ICommand command, 
            int timeoutSeconds, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            if (procedure == null) return default(T);

            var adoProcedure = procedure as IAdoProcedure;
            if (adoProcedure != null)
            {
                if (timeoutSeconds == 0)
                    return ExecuteScalar<T>(adoProcedure, command, connection, transaction);

                return ExecuteScalar<T>(adoProcedure, command, timeoutSeconds, connection, transaction);
            }

            var nativeProcedure = procedure as INativeProcedure;
            if (nativeProcedure != null)
            {
                throw new NotImplementedException("Procedures that access the SqLite engine natively are not supported in this version");
            }

            throw new Exception("Unknown procedure type '" + procedure.GetType().FullName + "'");
        }

        public long ExecuteNonQuery(
            IProcedure procedure, 
            ICommand command, 
            int timeoutSeconds, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            if (procedure == null) return 0;

            var adoProcedure = procedure as IAdoProcedure;
            if (adoProcedure != null)
            {
                if (timeoutSeconds == 0)
                    return ExecuteNonQuery(adoProcedure, command, connection, transaction);

                return ExecuteNonQuery(adoProcedure, command, timeoutSeconds, connection, transaction);
            }

            var nativeProcedure = procedure as INativeProcedure;
            if (nativeProcedure != null)
            {
                throw new NotImplementedException("Procedures that access the SqLite engine natively are not supported in this version");
            }

            throw new Exception("Unknown procedure type '" + procedure.GetType().FullName + "'");
        }

        private IDataReader ExecuteReader(
            IAdoProcedure procedure,
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string dataShapeName,
            Action<IDataReader> closeAction,
            Action<IDataReader> errorAction)
        {
            var context = CreateContext(command, connection, transaction, dataShapeName, closeAction, errorAction);
            try
            {
                return procedure.Execute(context);
            }
            catch (Exception e)
            {
                throw new AdoProcedureException(command, connection, e, "ExecuteReader");
            }
        }

        private IDataReader ExecuteReader(
            IAdoProcedure procedure,
            ICommand command,
            int timeoutSeconds,
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string dataShapeName,
            Action<IDataReader> closeAction,
            Action<IDataReader> errorAction)
        {
            var task = Task<IDataReader>.Factory.StartNew(() => ExecuteReader(procedure, command, connection, transaction, dataShapeName, closeAction, errorAction));
            return WaitForProcedureCompletion(command, timeoutSeconds, connection, task, "ExecuteReader");
        }

        private T ExecuteScalar<T>(
            IAdoProcedure procedure,
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            var context = CreateContext(command, connection, transaction, null, null, null);

            try
            {
                using (var dataReader = procedure.Execute(context))
                {
                    if (dataReader == null) return default(T);
                    if (!dataReader.Read()) return default(T);
                    return dataReader.Get<T>(0);
                }
            }
            catch (Exception e)
            {
                throw new AdoProcedureException(command, connection, e, "ExecuteScalar<" + typeof(T).Name + ">");
            }
        }

        private T ExecuteScalar<T>(
            IAdoProcedure procedure,
            ICommand command,
            int timeoutSeconds,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            var task = Task<T>.Factory.StartNew(() => ExecuteScalar<T>(procedure, command, connection, transaction));
            return WaitForProcedureCompletion(command, timeoutSeconds, connection, task, "ExecuteScalar<" + typeof(T).Name + ">");
        }

        private long ExecuteNonQuery(
            IAdoProcedure procedure,
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            var context = CreateContext(command, connection, transaction, null, null, null);

            try
            {
                using (procedure.Execute(context))
                {
                    return context.RowsAffected;
                }
            }
            catch (Exception e)
            {
                throw new AdoProcedureException(command, connection, e, "ExecuteNonQuery");
            }
        }

        private long ExecuteNonQuery(
            IAdoProcedure procedure,
            ICommand command,
            int timeoutSeconds,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            var task = Task<long>.Factory.StartNew(() => ExecuteNonQuery(procedure, command, connection, transaction));
            return WaitForProcedureCompletion(command, timeoutSeconds, connection, task, "ExecuteNonQuery");
        }

        private AdoExecutionContext CreateContext(
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string dataShapeName,
            Action<IDataReader> closeAction,
            Action<IDataReader> errorAction)
        {
            return new AdoExecutionContext
            {
                Connection = connection,
                Transaction = transaction,
                DataShapeName = dataShapeName,
                CloseAction = closeAction,
                ErrorAction = errorAction,
                Parameters = command == null ? null : command.GetParameters().ToList(),
                MessageOutput = new StringWriter(new StringBuilder())
            };
        }

        private T WaitForProcedureCompletion<T>(
            ICommand command,
            int timeoutSeconds,
            SQLiteConnection connection,
            Task<T> task,
            string operation)
        {
            try
            {
                if (task.Wait(timeoutSeconds*1000))
                {
                    return task.Result;
                }
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => !(e is AdoProcedureException));
            }
            finally
            {
                task.Dispose();
            }

            throw new AdoProcedureTimeoutException(command, connection, timeoutSeconds, operation);
        }

    }
}
