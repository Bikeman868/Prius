using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.Procedures
{
    internal class Runner : IProcedureRunner
    {
        public IDataReader ExecuteReader(
            IProcedure procedure, 
            ICommand command, 
            int timeout, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction, 
            string dataShapeName, 
            Action<IDataReader> closeAction, 
            Action<IDataReader> errorAction)
        {
            var adoProcedure = procedure as IAdoProcedure;
            if (adoProcedure != null)
            {
                return ExecuteReader(
                    adoProcedure,
                    command,
                    timeout,
                    connection,
                    transaction,
                    dataShapeName,
                    closeAction,
                    errorAction);
            }

            return null;
        }

        public T ExecuteScalar<T>(
            IProcedure procedure, 
            ICommand command, 
            int timeout, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            var adoProcedure = procedure as IAdoProcedure;
            if (adoProcedure != null)
            {
                return ExecuteScalar<T>(
                    adoProcedure,
                    command,
                    timeout,
                    connection,
                    transaction);
            }

            return default(T);
        }

        public long ExecuteNonQuery(
            IProcedure procedure, 
            ICommand command, 
            int timeout, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
            var adoProcedure = procedure as IAdoProcedure;
            if (adoProcedure != null)
            {
                return ExecuteNonQuery(
                    adoProcedure,
                    command,
                    timeout,
                    connection,
                    transaction);
            }

            return 0;
        }

        private IDataReader ExecuteReader(
            IAdoProcedure procedure,
            ICommand command,
            int timeout,
            SQLiteConnection connection,
            SQLiteTransaction transaction,
            string dataShapeName,
            Action<IDataReader> closeAction,
            Action<IDataReader> errorAction)
        {
            var context = new AdoExecutionContext
            {
                Connection = connection,
                Transaction = transaction,
                DataShapeName = dataShapeName,
                CloseAction = closeAction,
                ErrorAction = errorAction
            };
            return procedure.Execute(context);
        }

        private T ExecuteScalar<T>(
            IAdoProcedure procedure,
            ICommand command,
            int timeout,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            var context = new AdoExecutionContext
            {
                Connection = connection,
                Transaction = transaction
            };
            using (var dataReader = procedure.Execute(context))
            {
                if (dataReader == null) return default(T);
                if (!dataReader.Read()) return default(T);
                return dataReader.Get<T>(0);
            }
        }

        private long ExecuteNonQuery(
            IAdoProcedure procedure,
            ICommand command,
            int timeout,
            SQLiteConnection connection,
            SQLiteTransaction transaction)
        {
            var context = new AdoExecutionContext
            {
                Connection = connection,
                Transaction = transaction
            };
            using (var dataReader = procedure.Execute(context))
            {
                return context.RowsAffected;
            }
        }
    }
}
