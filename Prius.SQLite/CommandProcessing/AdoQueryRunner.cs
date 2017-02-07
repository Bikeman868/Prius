using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;
using Prius.SqLite.QueryBuilder;

namespace Prius.SqLite.CommandProcessing
{
    public class AdoQueryRunner: IAdoQueryRunner
    {
        private readonly IParameterConverter _parameterConverter;

        public AdoQueryRunner(IParameterConverter parameterConverter)
        {
            _parameterConverter = parameterConverter;
        }

        public int ExecuteNonQuery(SQLiteConnection connection, string sql, IList<IParameter> parameters)
        {
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
