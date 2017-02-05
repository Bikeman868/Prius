using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;
using Prius.Contracts.Interfaces.Connections;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.SchemaUpdating
{
    internal class SchemaUpdater : ISchemaUpdater
    {
        private readonly IQueryRunner _queryRunner;
        private readonly SortedList<string, string> _updatedRepositories;
        private readonly IDictionary<DbType, string> _dataTypeMap;

        private IList<TableSchema> _tables;

        public SchemaUpdater(
            IQueryRunner queryRunner)
        {
            _queryRunner = queryRunner;
            _updatedRepositories = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);

            _dataTypeMap = new Dictionary<DbType, string>();
            _dataTypeMap[DbType.AnsiString] = "TEXT";
            _dataTypeMap[DbType.AnsiStringFixedLength] = "TEXT";
            _dataTypeMap[DbType.Binary] = "BLOB";
            _dataTypeMap[DbType.Boolean] = "NUMERIC";
            _dataTypeMap[DbType.Byte] = "INTEGER";
            _dataTypeMap[DbType.Currency] = "NUMERIC";
            _dataTypeMap[DbType.Date] = "TEXT";
            _dataTypeMap[DbType.DateTime] = "TEXT";
            _dataTypeMap[DbType.DateTime2] = "TEXT";
            _dataTypeMap[DbType.DateTimeOffset] = "TEXT";
            _dataTypeMap[DbType.Decimal] = "NUMERIC";
            _dataTypeMap[DbType.Double] = "REAL";
            _dataTypeMap[DbType.Guid] = "TEXT";
            _dataTypeMap[DbType.Int16] = "INTEGER";
            _dataTypeMap[DbType.Int32] = "INTEGER";
            _dataTypeMap[DbType.Int64] = "INTEGER";
            _dataTypeMap[DbType.Object] = "BLOB";
            _dataTypeMap[DbType.SByte] = "INTEGER";
            _dataTypeMap[DbType.Single] = "REAL";
            _dataTypeMap[DbType.String] = "TEXT";
            _dataTypeMap[DbType.StringFixedLength] = "TEXT";
            _dataTypeMap[DbType.Time] = "INTEGER";
            _dataTypeMap[DbType.UInt16] = "INTEGER";
            _dataTypeMap[DbType.UInt32] = "INTEGER";
            _dataTypeMap[DbType.UInt64] = "INTEGER";
            _dataTypeMap[DbType.VarNumeric] = "REAL";
            _dataTypeMap[DbType.Xml] = "TEXT";
        }

        public void CheckSchema(IRepository repository, SQLiteConnection connection)
        {
            lock (_updatedRepositories)
            {
                if (_updatedRepositories.ContainsKey(repository.Name))
                    return;
                _updatedRepositories.Add(repository.Name, null);

                if (_tables == null)
                    _tables = ProbeForSchema();
            }

            var repositorySchema = _tables
                .Where(t => string.IsNullOrEmpty(t.RepositoryName) || string.Equals(t.RepositoryName, repository.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Check(repositorySchema, connection);
        }

        private void Check(IEnumerable<TableSchema> schema, SQLiteConnection connection)
        {
            var wasClosed = connection.State == ConnectionState.Closed;
            if (wasClosed) connection.Open();
            try
            {
                foreach (var table in schema)
                {
                    var currentSchema = GetCurrentSchema(connection, table.TableName);
                    if (currentSchema == null)
                        CreateTable(connection, table);
                    else
                        UpdateTable(connection, currentSchema, table);
                }
            }
            finally
            {
                if (wasClosed) connection.Close();
            }
        }


        private void CreateTable(SQLiteConnection connection, TableSchema tableSchema)
        {
            var sql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE {0} (", tableSchema.TableName);
            var separator = "";
            foreach (var column in tableSchema.Columns)
            {
                sql.AppendLine(separator);
                sql.Append("  ");
                AppendColumnDefinition(column, sql);
                separator = ",";
            }
            sql.AppendLine();
            sql.AppendLine(");");

            foreach (var index in tableSchema.Indexes)
            {
                sql.AppendFormat("CREATE {1}INDEX {0} ON {2} ({3})",
                    index.IndexName,
                    (index.Attributes & IndexAttributes.Unique) == IndexAttributes.Unique ? "UNIQUE " : "",
                    tableSchema.TableName,
                    string.Join(",", index.ColumnNames));
                sql.AppendLine(";");
            }

            _queryRunner.ExecuteNonQuery(connection, sql);
        }

        private void AppendColumnDefinition(ColumnSchema column, StringBuilder sql)
        {
            if ((column.Attributes & ColumnAttributes.AutoIncrement) == ColumnAttributes.AutoIncrement)
            {
                sql.AppendFormat("{0} INTEGER PRIMARY KEY AUTOINCREMENT", column.ColumnName, column.DataType);
            }
            else
            {
                sql.AppendFormat("{0} {1}", column.ColumnName, column.DataType);
                if ((column.Attributes & ColumnAttributes.Primary) == ColumnAttributes.Primary)
                {
                    sql.Append(" PRIMARY KEY");
                }
                else if ((column.Attributes & ColumnAttributes.NotNull) == ColumnAttributes.NotNull)
                {
                    sql.Append(" NOT NULL ON CONFLICT FAIL");
                }
                else if ((column.Attributes & ColumnAttributes.Unique) == ColumnAttributes.Unique)
                {
                    sql.Append(" UNIQUE ON CONFLICT FAIL");
                }
            }
        }

        private void UpdateTable(SQLiteConnection connection, TableSchema currentSchema, TableSchema newSchema)
        {
            var sql = new StringBuilder();

            // Delete current columns and recreate changed columns
            foreach (var currentColumn in currentSchema.Columns)
            {
                var newColumn = newSchema.Columns.FirstOrDefault(c => string.Equals(currentColumn.ColumnName, c.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (newColumn == null)
                {
                    var msg = string.Format(
                        "Existing database table '{0}' includes column '{1}' which is not in your current schema. " +
                        "SqLite does not have the ability to drop columns, you should add this column back into " +
                        "your current schema or write a data migration routine.",
                        currentSchema.TableName, currentColumn.ColumnName);
                    throw new Exception(msg);
                    /*
                    sql.Clear();
                    sql.AppendFormat("ALTER TABLE {0} DROP COLUMN {1}", currentSchema.TableName, currentColumn.ColumnName);
                    _queryRunner.ExecuteNonQuery(connection, sql);
                     */
                }
                else
                {
                    if (!string.Equals(currentColumn.DataType, newColumn.DataType, StringComparison.OrdinalIgnoreCase) ||
                        currentColumn.Attributes != newColumn.Attributes)
                    {
                    var msg = string.Format(
                        "The {1} column in the {0} table has a different schema in the database than the code is expecting. " +
                        "SqLite does not have the ability to alter column definitions, you should add a new column and " +
                        "write a data migration routine to copy the existing data over.",
                        currentSchema.TableName, currentColumn.ColumnName);
                    throw new Exception(msg);
                    }
                }
            }

            // Add new columns
            foreach (var newColumn in newSchema.Columns)
            {
                var currentColumn = currentSchema.Columns.FirstOrDefault(c => string.Equals(newColumn.ColumnName, c.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (currentColumn == null)
                {
                    sql.Clear();
                    sql.AppendFormat("ALTER TABLE {0} ADD COLUMN ", currentSchema.TableName);
                    AppendColumnDefinition(newColumn, sql);
                    _queryRunner.ExecuteNonQuery(connection, sql);
                }
            }
        }

        #region Schema

        private TableSchema GetCurrentSchema(SQLiteConnection connection, string tableName)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("SELECT sql FROM sqlite_master WHERE name='{0}'", tableName);

            var reader = _queryRunner.ExecuteReader(connection, sql);
            if (reader == null)
                return null;

            string createTableSql;
            using (reader)
            {
                if (!reader.Read())
                    return null;
                createTableSql = reader.GetString(0);
            }
            if (string.IsNullOrEmpty(createTableSql))
                return null;

            var tableSchema = ParseCreateTable(createTableSql);

            // TODO: Retrieve and parse index definitions

            return tableSchema;
        }

        private TableSchema ParseCreateTable(string sql)
        {
            sql = sql.Replace("\n", "")
                .Replace("\r", "")
                .Replace("     ", " ")
                .Replace("    ", " ")
                .Replace("   ", " ")
                .Replace("  ", " ")
                .Replace(", ", ",")
                .Replace(") ", ")")
                .Replace(" )", ")")
                .Replace("( ", "(")
                .Replace(" (", "(");

            var index = 0;

            Func<bool> atEnd = () => index >= sql.Length;
            Action<int> skip = n => index += n;
            Action<string> skipToString = s => index = sql.IndexOf(s, index, StringComparison.OrdinalIgnoreCase);
            Func<char[], string> takeToAny = c =>
            {
                var end = sql.IndexOfAny(c, index);
                var result = end == -1 ? sql.Substring(index) : sql.Substring(index, end - index);
                skip(result.Length);
                return result;
            };
            Action<string> skipOverString = s => 
            { 
                skipToString(s); skip(s.Length); 
            };
            Func<string> takeWord = () =>
            {
                var separator = new List<char> {' ', ',', '(', ')'};
                while (!atEnd() && separator.Contains(sql[index])) index++;
                var end = index + 1;
                while (end < sql.Length && !separator.Contains(sql[end])) end++;
                var result = sql.Substring(index, end - index);
                skip(result.Length);
                return result;
            };

            var tableSchema = new TableSchema();
            tableSchema.Columns = new List<ColumnSchema>();
            tableSchema.Indexes = new List<IndexSchema>();

            skipOverString("create table");
            tableSchema.TableName = takeWord();
            skipOverString("(");
            while (!atEnd() && sql[index] != ')')
            {
                var column = new ColumnSchema();
                tableSchema.Columns.Add(column);

                column.ColumnName = takeWord();
                column.DataType = takeWord();

                var attributes = takeToAny(new[]{',', ')'}).ToUpper();
                if (attributes.Contains("PRIMARY KEY"))
                    column.Attributes = column.Attributes | ColumnAttributes.Primary;
                if (attributes.Contains("NOT NULL"))
                    column.Attributes = column.Attributes | ColumnAttributes.NotNull;
                if (attributes.Contains("UNIQUE"))
                    column.Attributes = column.Attributes | ColumnAttributes.Unique;
                if (attributes.Contains("AUTOINCREMENT"))
                    column.Attributes = column.Attributes | ColumnAttributes.AutoIncrement;
            }

            return tableSchema;
        }

        private IList<TableSchema> ProbeForSchema()
        {
            var tables = new List<TableSchema>();

            tables.Add(
                new TableSchema
                {
                    RepositoryName = "",
                    TableName = "tb_Users",
                    Columns = new List<ColumnSchema> 
                    { 
                        new ColumnSchema { ColumnName = "UserID", DataType = _dataTypeMap[DbType.UInt32], Attributes = ColumnAttributes.UniqueKey },
                        new ColumnSchema { ColumnName = "FirstName", DataType = _dataTypeMap[DbType.String], Attributes = ColumnAttributes.None },
                        new ColumnSchema { ColumnName = "LastName", DataType = _dataTypeMap[DbType.String], Attributes = ColumnAttributes.None },
                    },
                    Indexes = new List<IndexSchema> 
                    { 
                        new IndexSchema 
                        { 
                            IndexName = "ix_FullName", 
                            Attributes = IndexAttributes.Unique, 
                            ColumnNames = new[] { "FirstName", "LastName" }
                        }
                    }
                });

            return tables;
        }

        private class TableSchema
        {
            public string RepositoryName { get; set; }
            public string TableName { get; set; }
            public IList<ColumnSchema> Columns { get; set; }
            public IList<IndexSchema> Indexes { get; set; }
        }

        private class ColumnSchema
        {
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public ColumnAttributes Attributes { get; set; }
        }

        private class IndexSchema
        {
            public string IndexName { get; set; }
            public IndexAttributes Attributes { get; set; }
            public string[] ColumnNames { get; set; }
        }

        #endregion
    }
}
