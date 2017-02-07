using System;
using System.Text;

namespace Prius.SqLite.QueryBuilder
{
    internal class QueryBuilder : IQueryBuilder
    {
        ISelectQueryBuilder IQueryBuilder.Select(params string[] fields)
        {
            return new Query("SELECT " + string.Join(", ", fields));
        }

        ISelectQueryBuilder IQueryBuilder.SelectDistinct(params string[] fields)
        {
            return new Query("SELECT DISTINCT " + string.Join(", ", fields));
        }

        IDeleteQueryBuilder IQueryBuilder.DeleteFrom(string tableName)
        {
            return new Query("DELETE FROM " + tableName);
        }

        IUpdateQueryBuilder IQueryBuilder.Update(string tableName)
        {
            return new Query("UPDATE " + tableName);
        }

        IUpdateQueryBuilder IQueryBuilder.UpdateOrRollback(string tableName)
        {
            return new Query("UPDATE OR ROLLBACK " + tableName);
        }

        IUpdateQueryBuilder IQueryBuilder.UpdateOrAbort(string tableName)
        {
            return new Query("UPDATE OR ABORT " + tableName);
        }

        IUpdateQueryBuilder IQueryBuilder.UpdateOrReplace(string tableName)
        {
            return new Query("UPDATE OR REPLACE " + tableName);
        }

        IUpdateQueryBuilder IQueryBuilder.UpdateOrFail(string tableName)
        {
            return new Query("UPDATE OR FAIL " + tableName);
        }

        IUpdateQueryBuilder IQueryBuilder.UpdateOrIgnore(string tableName)
        {
            return new Query("UPDATE OR IGNORE " + tableName);
        }

        IInsertQueryBuilder IQueryBuilder.InsertInto(string tableName, params string[] fields)
        {
            return new Query("INSERT INTO " + tableName + " (" + string.Join(", ", fields) + ")");
        }

        IInsertQueryBuilder IQueryBuilder.InsertOrReplaceInto(string tableName, params string[] fields)
        {
            return new Query("INSERT OR REPLACE INTO " + tableName + " (" + string.Join(", ", fields) + ")");
        }

        IInsertQueryBuilder IQueryBuilder.InsertOrRollbackInto(string tableName, params string[] fields)
        {
            return new Query("INSERT OR ROLLBACK INTO " + tableName + " (" + string.Join(", ", fields) + ")");
        }

        IInsertQueryBuilder IQueryBuilder.InsertOrAbortInto(string tableName, params string[] fields)
        {
            return new Query("INSERT OR ABORT INTO " + tableName + " (" + string.Join(", ", fields) + ")");
        }

        IInsertQueryBuilder IQueryBuilder.InsertOrFailInto(string tableName, params string[] fields)
        {
            return new Query("INSERT OR FAIL INTO " + tableName + " (" + string.Join(", ", fields) + ")");
        }

        IInsertQueryBuilder IQueryBuilder.InsertOrIgnoreInto(string tableName, params string[] fields)
        {
            return new Query("INSERT OR IGNORE INTO " + tableName + " (" + string.Join(", ", fields) + ")");
        }

        IInsertQueryBuilder IQueryBuilder.ReplaceInto(string tableName, params string[] fields)
        {
            return new Query("REPLACE INTO " + tableName + " (" + string.Join(", ", fields) + ")");
        }

        internal class Query :
            ISelectQueryBuilder,
            ISelectFromQueryBuilder,
            ISelectWhereQueryBuilder,
            ISelectGroupQueryBuilder,
            ISelectOrderQueryBuilder,
            ISelectOrderCollateQueryBuilder,

            IDeleteQueryBuilder,
            IDeleteWhereQueryBuilder,

            IUpdateQueryBuilder,
            IUpdateSetQueryBuilder,
            IUpdateWhereQueryBuilder,

            IInsertQueryBuilder
        {
            private StringBuilder _sql = new StringBuilder();

            public Query(string text)
            {
                _sql = new StringBuilder();
                _sql.Append(text);
            }

            public override string ToString()
            {
                return _sql.ToString();
            }

            string IQuery.ToString()
            {
                return _sql.ToString();
            }

            #region SELECT statements

            ISelectFromQueryBuilder ISelectQueryBuilder.From(string tableName)
            {
                _sql.AppendFormat(" FROM {0}", tableName);
                return this;
            }

            ISelectFromQueryBuilder ISelectFromQueryBuilder.Join(string leftTable, string leftField, string rightTable,
                string rightField)
            {
                _sql.AppendFormat(" JOIN {2} ON {0}.{1} = {2}.{3}", leftTable, leftField, rightTable, rightField);
                return this;
            }

            ISelectFromQueryBuilder ISelectFromQueryBuilder.LeftJoin(string leftTable, string leftField,
                string rightTable, string rightField)
            {
                _sql.AppendFormat(" LEFT JOIN {2} ON {0}.{1} = {2}.{3}", leftTable, leftField, rightTable, rightField);
                return this;
            }

            ISelectWhereQueryBuilder ISelectFromQueryBuilder.Where(string condition)
            {
                _sql.AppendFormat(" WHERE ({0})", condition);
                return this;
            }

            ISelectOrderQueryBuilder ISelectFromQueryBuilder.OrderBy(params string[] fieldNames)
            {
                _sql.Append(" ORDER BY " + string.Join(", ", fieldNames));
                return this;
            }

            ISelectGroupQueryBuilder ISelectFromQueryBuilder.GroupBy(params string[] fieldNames)
            {
                _sql.Append(" GROUP BY " + string.Join(", ", fieldNames));
                return this;
            }

            ISelectWhereQueryBuilder ISelectWhereQueryBuilder.And(string condition)
            {
                _sql.AppendFormat(" AND ({0})", condition);
                return this;
            }

            ISelectOrderQueryBuilder ISelectWhereQueryBuilder.OrderBy(params string[] fieldNames)
            {
                _sql.Append(" ORDER BY " + string.Join(", ", fieldNames));
                return this;
            }

            ISelectOrderCollateQueryBuilder ISelectOrderQueryBuilder.Collate(string collation)
            {
                _sql.Append(" COLLATE " + collation);
                return this;
            }

            ISelectOrderQueryBuilder ISelectOrderQueryBuilder.Ascending()
            {
                _sql.Append(" ASC");
                return this;
            }

            ISelectOrderQueryBuilder ISelectOrderQueryBuilder.Descending()
            {
                _sql.Append(" DESC");
                return this;
            }

            ISelectOrderQueryBuilder ISelectOrderQueryBuilder.ThenBy(string fieldName)
            {
                _sql.Append(", " + fieldName);
                return this;
            }

            IQuery ISelectOrderQueryBuilder.Limit(int rowCount)
            {
                _sql.Append(" LIMIT " + rowCount);
                return this;
            }

            ISelectOrderQueryBuilder ISelectOrderCollateQueryBuilder.Ascending()
            {
                _sql.Append(" ASC");
                return this;
            }

            ISelectOrderQueryBuilder ISelectOrderCollateQueryBuilder.Descending()
            {
                _sql.Append(" DESC");
                return this;
            }

            ISelectOrderQueryBuilder ISelectOrderCollateQueryBuilder.ThenBy(string fieldName)
            {
                _sql.Append(", " + fieldName);
                return this;
            }

            IQuery ISelectOrderCollateQueryBuilder.Limit(int rowCount)
            {
                _sql.Append(" LIMIT " + rowCount);
                return this;
            }

            ISelectOrderQueryBuilder ISelectGroupQueryBuilder.Having(string expression)
            {
                _sql.Append(" HAVING " + expression);
                return this;
            }

            ISelectOrderQueryBuilder ISelectGroupQueryBuilder.OrderBy(params string[] fieldNames)
            {
                _sql.Append(" ORDER BY " + string.Join(", ", fieldNames));
                return this;
            }

            #endregion

            #region DELETE statement

            IDeleteWhereQueryBuilder IDeleteQueryBuilder.Where(string condition)
            {
                _sql.AppendFormat(" WHERE ({0})", condition);
                return this;
            }

            IDeleteWhereQueryBuilder IDeleteWhereQueryBuilder.And(string condition)
            {
                _sql.AppendFormat(" AND ({0})", condition);
                return this;
            }

            #endregion

            #region UPDATE statement

            IUpdateSetQueryBuilder IUpdateQueryBuilder.Set(string field, string value)
            {
                _sql.AppendFormat(" SET {0} = {1}", field, value);
                return this;
            }

            IUpdateSetQueryBuilder IUpdateSetQueryBuilder.Set(string field, string value)
            {
                _sql.AppendFormat(", {0} = {1}", field, value);
                return this;
            }

            IUpdateSetQueryBuilder IUpdateSetQueryBuilder.Where(string condition)
            {
                _sql.AppendFormat(" WHERE ({0})", condition);
                return this;
            }

            IUpdateWhereQueryBuilder IUpdateWhereQueryBuilder.And(string condition)
            {
                _sql.AppendFormat(" AND ({0})", condition);
                return this;
            }

            #endregion

            #region INSERT statement

            IQuery IInsertQueryBuilder.Values(params string[] values)
            {
                _sql.AppendFormat(" VALUES ({0})", string.Join(", ", values));
                return this;
            }

            IQuery IInsertQueryBuilder.DefaultValues()
            {
                _sql.Append(" DEFAULT VALUES");
                return this;
            }

            ISelectQueryBuilder IInsertQueryBuilder.Select(params string[] fields)
            {
                _sql.Append(" SELECT ");
                _sql.Append(string.Join(", ", fields));
                return this;
            }

            ISelectQueryBuilder IInsertQueryBuilder.SelectDistinct(params string[] fields)
            {
                _sql.Append(" SELECT DISTINCT ");
                _sql.Append(string.Join(", ", fields));
                return this;
            }

            #endregion
        }
    }
}
