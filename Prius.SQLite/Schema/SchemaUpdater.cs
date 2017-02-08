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
    /// <summary>
    /// Compares the actual database schema with the schema defined within the application
    /// and modifies the database schema to match the application.
    /// 
    /// Note that you can not rename tables. Changing the name of the table in your
    /// schema attributes will provide no information about what that table used to
    /// be called.
    /// </summary>
    internal class SchemaUpdater : ISchemaUpdater
    {
        private readonly IAdoQueryRunner _queryRunner;
        private readonly ISchemaEnumerator _schemaEnumerator;

        private readonly SortedList<string, string> _updatedRepositories;

        private IList<TableSchema> _tables;

        public SchemaUpdater(
            IAdoQueryRunner queryRunner, 
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

            CreateTableDdl(tableSchema, sql);
            sql.AppendLine(";");

            foreach (var index in tableSchema.Indexes)
            {
                CreateIndexDdl(tableSchema, index, sql);
                sql.AppendLine(";");
            }

            _queryRunner.ExecuteNonQuery(connection, sql.ToString());
        }

        private void UpdateTable(SQLiteConnection connection, TableSchema currentTableSchema, TableSchema newTableSchema)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var hasBreakingChanges = HasBreakingChanges(currentTableSchema, newTableSchema);

                    if (hasBreakingChanges)
                    {
                        RecreateTable(connection, currentTableSchema, newTableSchema);
                    }
                    else
                    {
                        AddMissingColumns(connection, currentTableSchema, newTableSchema);
                        AdjustExistingIndexes(connection, currentTableSchema, newTableSchema);
                        AddMissingIndexes(connection, currentTableSchema, newTableSchema);
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private void AddMissingIndexes(
            SQLiteConnection connection,
            TableSchema currentTableSchema,
            TableSchema newTableSchema)
        {
            var sql = new StringBuilder();

            foreach (var newIndex in newTableSchema.Indexes)
            {
                var currentIndex =
                    currentTableSchema.Indexes.FirstOrDefault(
                        c => string.Equals(newIndex.IndexName, c.IndexName, StringComparison.OrdinalIgnoreCase));
                if (currentIndex == null)
                {
                    CreateIndexDdl(currentTableSchema, newIndex, sql);
                    sql.AppendLine(";");
                }
            }

            _queryRunner.ExecuteNonQuery(connection, sql.ToString());
        }

        private void AdjustExistingIndexes(
            SQLiteConnection connection,
            TableSchema currentTableSchema, 
            TableSchema newTableSchema)
        {
            var sql = new StringBuilder();

            foreach (var currentIndex in currentTableSchema.Indexes)
            {
                var newIndex = newTableSchema.Indexes.FirstOrDefault(i => string.Equals(
                    currentIndex.IndexName, i.IndexName, StringComparison.OrdinalIgnoreCase));
                if (newIndex == null)
                {
                    DropIndexDdl(currentIndex, sql);
                    sql.AppendLine(";");
                }
                else if (currentIndex.Attributes != newIndex.Attributes ||
                    currentIndex.ColumnNames.Length != newIndex.ColumnNames.Length)
                {
                    DropIndexDdl(currentIndex, sql);
                    sql.AppendLine(";");
                    CreateIndexDdl(currentTableSchema, newIndex, sql);
                    sql.AppendLine(";");
                }
                else
                {
                    var columnsVary = currentIndex.ColumnNames
                        .Where((t, i) => !string.Equals(t, newIndex.ColumnNames[i], StringComparison.OrdinalIgnoreCase))
                        .Any();
                    if (columnsVary)
                    {
                        DropIndexDdl(currentIndex, sql);
                        sql.AppendLine(";");
                        CreateIndexDdl(currentTableSchema, newIndex, sql);
                        sql.AppendLine(";");
                    }
                }
            }

            _queryRunner.ExecuteNonQuery(connection, sql.ToString());
        }

        private void AddMissingColumns(
            SQLiteConnection connection,
            TableSchema currentTableSchema, 
            TableSchema newTableSchema)
        {
            var sql = new StringBuilder();

            foreach (var newColumn in newTableSchema.Columns)
            {
                var currentColumn =
                    currentTableSchema.Columns.FirstOrDefault(
                        c => string.Equals(newColumn.ColumnName, c.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (currentColumn == null)
                {
                    AddColumnDdl(currentTableSchema, newColumn, sql);
                    sql.AppendLine(";");
                }
            }

            _queryRunner.ExecuteNonQuery(connection, sql.ToString());
        }

        private void RecreateTable(
            SQLiteConnection connection, 
            TableSchema currentTableSchema, 
            TableSchema newTableSchema)
        {
            var sql = new StringBuilder();

            // Drop all indexes
            foreach (var index in currentTableSchema.Indexes)
            {
                DropIndexDdl(index, sql);
                sql.AppendLine(";");
            }

            var tempTableName = "TMP_" + Guid.NewGuid().ToString("N");
            sql.Clear();

            // Rename the existing table
            RenameTableDdl(currentTableSchema.TableName, tempTableName, sql);
            sql.AppendLine(";");

            // Create a new table
            CreateTableDdl(newTableSchema, sql);
            sql.AppendLine(";");

            // Copy the data from the old table to the new one
            var currentColumnNames = currentTableSchema.Columns
                .Select(c => c.ColumnName)
                .ToList();
            var newColumnNames = newTableSchema.Columns
                .Select(c => c.ColumnName)
                .ToList();
            var columnsToCopy = currentColumnNames
                .Where(c => newColumnNames.Contains(c, StringComparer.OrdinalIgnoreCase))
                .ToList();
            sql.AppendFormat("INSERT OR REPLACE INTO {0} ({1}) SELECT {1} FROM {2}",
                newTableSchema.TableName, string.Join(", ", columnsToCopy), tempTableName);
            sql.AppendLine(";");

            // Drop the renamed old table
            DropTableDdl(tempTableName, sql);
            sql.AppendLine(";");

            // Add indexes to the new table
            foreach (var index in newTableSchema.Indexes)
            {
                CreateIndexDdl(newTableSchema, index, sql);
                sql.AppendLine(";");
            }

            _queryRunner.ExecuteNonQuery(connection, sql.ToString());
        }

        private static bool HasBreakingChanges(TableSchema currentTableSchema, TableSchema newTableSchema)
        {
            foreach (var currentColumn in currentTableSchema.Columns)
            {
                var newColumn = newTableSchema.Columns.FirstOrDefault(
                        c => string.Equals(currentColumn.ColumnName, c.ColumnName, StringComparison.OrdinalIgnoreCase));
                if (newColumn == null)
                {
                    return true;
                }
                if (!string.Equals(currentColumn.DataType, newColumn.DataType, StringComparison.OrdinalIgnoreCase) ||
                    currentColumn.Attributes != newColumn.Attributes)
                {
                    return true;
                }
            }
            return false;
        }

        #region DDL queries

        private void AddColumnDdl(TableSchema table, ColumnSchema column, StringBuilder sql)
        {
            sql.AppendFormat("ALTER TABLE {0} ADD COLUMN ", table.TableName);
            ColumnDdl(column, sql);
        }

        private void CreateTableDdl(TableSchema table, StringBuilder sql)
        {
            sql.AppendFormat("CREATE TABLE {0} (", table.TableName);
            var separator = "";
            foreach (var column in table.Columns)
            {
                sql.AppendLine(separator);
                sql.Append("  ");
                ColumnDdl(column, sql);
                separator = ",";
            }
            sql.AppendLine();
            sql.Append(")");
        }

        private void CreateIndexDdl(TableSchema table, IndexSchema index, StringBuilder sql)
        {
            var collate = ((index.Attributes & IndexAttributes.CaseSensitive) == IndexAttributes.CaseSensitive) ? "" : " COLLATE NOCASE";
            sql.AppendFormat("CREATE {1}INDEX {0} ON {2} ({3})",
                index.IndexName,
                (index.Attributes & IndexAttributes.Unique) == IndexAttributes.Unique ? "UNIQUE " : "",
                table.TableName,
                string.Join(",", index.ColumnNames.Select(c => c + collate)));
        }

        private void DropIndexDdl(IndexSchema index, StringBuilder sql)
        {
            sql.AppendFormat("DROP INDEX {0}", index.IndexName);
        }

        private void RenameTableDdl(string oldName, string newName, StringBuilder sql)
        {
            sql.AppendFormat("ALTER TABLE {0} RENAME TO {1}", oldName, newName);
        }

        private void DropTableDdl(string tableName, StringBuilder sql)
        {
            sql.AppendFormat("DROP TABLE {0}", tableName);
        }

        private void ColumnDdl(ColumnSchema column, StringBuilder sql)
        {
            if ((column.Attributes & ColumnAttributes.AutoIncrement) == ColumnAttributes.AutoIncrement)
            {
                sql.AppendFormat("{0} INTEGER PRIMARY KEY AUTOINCREMENT", column.ColumnName);
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

        #endregion

        #region GetCurrentSchema

        private TableSchema GetCurrentSchema(SQLiteConnection connection, string tableName)
        {
            var tableSql = string.Format("SELECT sql FROM sqlite_master WHERE type='table' AND name='{0}'", tableName);
            var reader = _queryRunner.ExecuteReader(connection, tableSql);
            if (reader == null) return null;

            TableSchema tableSchema = null;
            using (reader)
            {
                if (reader.Read())
                {
                    var createTableSql = reader.GetString(0);
                    if (string.IsNullOrEmpty(createTableSql)) return null;
                    tableSchema = ParseCreateTable(createTableSql);
                }
            }
            if (tableSchema == null) return null;

            tableSchema.Indexes = new List<IndexSchema>();

            var indexSql = string.Format("SELECT sql FROM sqlite_master WHERE type='index' AND tbl_name='{0}'", tableName);
            reader = _queryRunner.ExecuteReader(connection, indexSql);
            if (reader != null)
            {
                using (reader)
                {
                    while (reader.Read())
                    {
                        var createIndexSql = reader.GetString(0);
                        var indexSchema = ParseCreateIndex(createIndexSql);
                        tableSchema.Indexes.Add(indexSchema);
                    }
                }
            }

            return tableSchema;
        }

        private IndexSchema ParseCreateIndex(string sql)
        {
            var parser = new SimpleParser(sql);
            var indexSchema = new IndexSchema();
            indexSchema.Attributes = IndexAttributes.CaseSensitive;

            parser.SkipOverString("CREATE");
            if (parser.TakeWord().ToUpper() == "UNIQUE")
            {
                indexSchema.Attributes = indexSchema.Attributes | IndexAttributes.Unique;
                parser.SkipOverString("INDEX");
            }

            indexSchema.IndexName = parser.TakeWord();
            if (indexSchema.IndexName.ToUpper() == "IF")
            {
                parser.SkipOverString("EXISTS");
                indexSchema.IndexName = parser.TakeWord();
            }
            parser.SkipOverString("ON");
            parser.TakeWord(); // table name
            parser.SkipOverString("(");

            var columnNames = new List<string>();
            while (!parser.AtEnd() && !parser.Is(')'))
            {
                columnNames.Add(parser.TakeWord());
                parser.SkipAny(' ');
                var columnOptions = parser.TakeToAny(',', ')').ToUpper();
                if (columnOptions.Contains("COLLATE NOCASE"))
                    indexSchema.Attributes = indexSchema.Attributes & ~IndexAttributes.CaseSensitive;
                parser.Skip(1);
            }
            indexSchema.ColumnNames = columnNames.ToArray();

            return indexSchema;
        }

        private TableSchema ParseCreateTable(string sql)
        {
            var parser = new SimpleParser(sql);

            var tableSchema = new TableSchema();
            tableSchema.Columns = new List<ColumnSchema>();

            parser.SkipOverString("create table");
            tableSchema.TableName = parser.TakeWord();
            parser.SkipOverString("(");
            while (!parser.AtEnd() && !parser.Is(')'))
            {
                var column = new ColumnSchema();
                tableSchema.Columns.Add(column);

                column.ColumnName = parser.TakeWord();
                column.DataType = parser.TakeWord();

                var attributes = parser.TakeToAny(new[]{',', ')'}).ToUpper();
                if (attributes.Contains("PRIMARY KEY"))
                    column.Attributes = column.Attributes | ColumnAttributes.Primary;
                if (attributes.Contains("NOT NULL"))
                    column.Attributes = column.Attributes | ColumnAttributes.NotNull;
                if (attributes.Contains("UNIQUE"))
                    column.Attributes = column.Attributes | ColumnAttributes.Unique;
                if (attributes.Contains("AUTOINCREMENT"))
                    column.Attributes = column.Attributes | ColumnAttributes.AutoIncrement;
                if (string.Equals(column.DataType, "TEXT", StringComparison.OrdinalIgnoreCase) && !attributes.Contains("COLLATE NOCASE"))
                    column.Attributes = column.Attributes | ColumnAttributes.CaseSensitive;
            }

            return tableSchema;
        }

        private class SimpleParser
        {
            private readonly string _sql;
            private int _position;

            public SimpleParser(string sql)
            {
                _sql = sql.Replace("\n", "")
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
            }

            public bool AtEnd()
            {
                return _position >= _sql.Length;
            }

            public bool Is(char c)
            {
                return _sql[_position] == c;
            }

            public bool IsAny(params char[] chars)
            {
                return chars.Contains(_sql[_position]);
            }

            public void Skip(int count)
            {
                _position += count;
            }

            public void SkipToString(string s)
            {
                _position = _sql.IndexOf(s, _position, StringComparison.OrdinalIgnoreCase);
            }

            public void SkipAny(params char[] chars)
            {
                while (IsAny(chars)) Skip(1);
            }

            public string TakeToAny(params char[] chars)
            {
                var end = _sql.IndexOfAny(chars, _position);
                if (end == _position) return string.Empty;
                var result = end == -1 ? _sql.Substring(_position) : _sql.Substring(_position, end - _position);
                Skip(result.Length);
                return result;
            }

            public void SkipOverString(string s)
            {
                SkipToString(s);
                Skip(s.Length);
            }

            public string TakeWord()
            {
                var separator = new List<char> { ' ', ',', '(', ')' };
                while (!AtEnd() && separator.Contains(_sql[_position])) _position++;
                var end = _position + 1;
                while (end < _sql.Length && !separator.Contains(_sql[end])) end++;
                var result = _sql.Substring(_position, end - _position);
                Skip(result.Length);
                return result;
            }
        }

        #endregion
    }
}
