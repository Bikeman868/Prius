using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Prius.Contracts.Interfaces.Connections;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.Schema
{
    internal class SchemaUpdater : ISchemaUpdater
    {
        private readonly IQueryRunner _queryRunner;
        private readonly ISchemaEnumerator _schemaEnumerator;

        private readonly SortedList<string, string> _updatedRepositories;

        private IList<TableSchema> _tables;

        public SchemaUpdater(
            IQueryRunner queryRunner, 
            ISchemaEnumerator schemaEnumerator)
        {
            _queryRunner = queryRunner;
            _schemaEnumerator = schemaEnumerator;
            _updatedRepositories = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void CheckSchema(IRepository repository, SQLiteConnection connection)
        {
            lock (_updatedRepositories)
            {
                if (_updatedRepositories.ContainsKey(repository.Name))
                    return;
                _updatedRepositories.Add(repository.Name, null);

                if (_tables == null)
                    _tables = _schemaEnumerator.EnumerateTableSchemas();
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
                var collate = ((index.Attributes & IndexAttributes.CaseSensitive) == IndexAttributes.CaseSensitive) ? "" : " COLLATE NOCASE";
                sql.AppendFormat("CREATE {1}INDEX {0} ON {2} ({3})",
                    index.IndexName,
                    (index.Attributes & IndexAttributes.Unique) == IndexAttributes.Unique ? "UNIQUE " : "",
                    tableSchema.TableName,
                    string.Join(",", index.ColumnNames.Select(c => c + collate)));
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
                if ((column.Attributes & ColumnAttributes.NotNull) == ColumnAttributes.NotNull)
                {
                    sql.Append(" NOT NULL ON CONFLICT FAIL");
                }
                if ((column.Attributes & ColumnAttributes.Unique) == ColumnAttributes.Unique)
                {
                    sql.Append(" UNIQUE ON CONFLICT FAIL");
                }
                if (string.Equals(column.DataType, "TEXT", StringComparison.OrdinalIgnoreCase) &&
                    (column.Attributes & ColumnAttributes.CaseSensitive) != ColumnAttributes.CaseSensitive)
                {
                    sql.Append(" COLLATE NOCASE");
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

            // TODO: To delete or change columns we need to rename the existing table, create a new table with the original name,
            // copy all the data accross then delete the old table.

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

        #region GetCurrentSchema

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
                if (!attributes.Contains("COLLATE NOCASE"))
                    column.Attributes = column.Attributes | ColumnAttributes.CaseSensitive;
            }

            return tableSchema;
        }

        #endregion
    }
}
