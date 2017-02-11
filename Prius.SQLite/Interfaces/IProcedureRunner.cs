using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// Encapsulates the functionallity of executing stored procedures
    /// and returning the output in different forms. For execute scalar
    /// and execute non query, the request is completely finished after
    /// the stored procedure has run. For the case of execute reader, the
    /// connection to the database remains open so that the results can
    /// be fetched as they are processed.
    /// </summary>
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
