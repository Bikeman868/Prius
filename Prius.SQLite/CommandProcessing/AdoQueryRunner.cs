using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Utility;
using Prius.SQLite.Interfaces;
using Prius.SQLite.Procedures;
using Prius.SQLite.QueryBuilder;

#if DEBUG
using System.Diagnostics;
#endif

namespace Prius.SQLite.CommandProcessing
{
    /// <summary>
    /// This is designed to be injected into stored procedures to make
    /// it easier for them to execute queries against the sqlite database.
    /// The stored procedure always has an open connection to the database
    /// which is passed in.
    /// The ADO Query runner uses the SQLite ADO.Net driver in 
    /// System.Data.SQLite.
    /// There is also a native query runner that talks directly to the SQLite
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

        public T ExecuteScaler<T>(SQLiteConnection connection, string sql, IList<IParameter> parameters)
        {
            return GetScalar<T>(ExecuteReader(connection, sql, parameters));
        }

        public T ExecuteScaler<T>(AdoExecutionContext context, string sql)
        {
            return GetScalar<T>(ExecuteReader(context, sql));
        }

        public T ExecuteScaler<T>(AdoExecutionContext context, IQuery sql)
        {
            return GetScalar<T>(ExecuteReader(context, sql));
        }

        private T GetScalar<T>(SQLiteDataReader sqLiteDataReader)
        {
            using (sqLiteDataReader)
            {
                if (!sqLiteDataReader.Read()) return default(T);

                var value = sqLiteDataReader.GetValue(0);
                if (ReferenceEquals(value, null)) return default(T);

                var type = typeof(T);
                if (type.IsNullable())
                    type = type.GetGenericArguments()[0];
                if (type.IsEnum) type = typeof(int);

                return (T)Convert.ChangeType(value, type);
            }
        }
    }
}
