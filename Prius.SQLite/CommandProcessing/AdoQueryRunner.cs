using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;
using Prius.SqLite.QueryBuilder;

namespace Prius.SqLite.CommandProcessing
{
    /// <summary>
    /// This is designed to be injected into stored procedures to make
    /// it easier for them to execute queries against the sqlite database.
    /// The stored procedure always has an open connection to the database
    /// which is passed in.
    /// The ADO Query runner uses the SqLite ADO.Net driver in 
    /// System.Data.SQLite.
    /// There is also a native query runner that talks directly to the SqLite
    /// engine.
    /// </summary>
    public class AdoQueryRunner: IAdoQueryRunner
    {
        private readonly IParameterConverter _parameterConverter;

        public AdoQueryRunner(IParameterConverter parameterConverter)
        {
            _parameterConverter = parameterConverter;
        }

        public int ExecuteNonQuery(SQLiteConnection connection, string sql, IList<IParameter> parameters)
        {
            if (connection == null || string.IsNullOrEmpty(sql)) return 0;

#if DEBUG
            Trace.WriteLine(sql);
#endif

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                if (parameters != null)
                    foreach (var parameter in parameters)
                        _parameterConverter.AddParameter(command, parameter);
                return command.ExecuteNonQuery();
            }
        }

        public SQLiteDataReader ExecuteReader(SQLiteConnection connection, string sql, IList<IParameter> parameters)
        {
            if (connection == null || string.IsNullOrEmpty(sql)) return null;

#if DEBUG
            Trace.WriteLine(sql);
#endif

            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                if (parameters != null)
                    foreach (var parameter in parameters)
                        _parameterConverter.AddParameter(command, parameter);
                return command.ExecuteReader();
            }
        }

        public int ExecuteNonQuery(AdoExecutionContext context, string sql)
        {
            context.RowsAffected = ExecuteNonQuery(context.Connection, sql, context.Parameters);
            return context.RowsAffected;
        }

        public SQLiteDataReader ExecuteReader(AdoExecutionContext context, string sql)
        {
            return ExecuteReader(context.Connection, sql, context.Parameters);
        }

        public int ExecuteNonQuery(AdoExecutionContext context, IQuery sql)
        {
            return ExecuteNonQuery(context, sql.ToString());
        }

        public SQLiteDataReader ExecuteReader(AdoExecutionContext context, IQuery sql)
        {
            return ExecuteReader(context, sql.ToString());
        }
    }
}
