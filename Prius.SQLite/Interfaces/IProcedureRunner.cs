using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    public interface IProcedureRunner
    {
        IDataReader ExecuteReader(
            IProcedure procedure, 
            ICommand command, 
            int timeout, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction, 
            string dataShapeName,
            Action<IDataReader> closeAction, 
            Action<IDataReader> errorAction);

        T ExecuteScalar<T>(
            IProcedure procedure, 
            ICommand command, 
            int timeout, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction);

        long ExecuteNonQuery(
            IProcedure procedure, 
            ICommand command, 
            int timeout, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction);
    }
}
