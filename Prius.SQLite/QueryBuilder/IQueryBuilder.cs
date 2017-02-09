namespace Prius.SqLite.QueryBuilder
{
    /// <summary>
    /// Understands basic SqLite SQL syntax and assists developers in
    /// writing syntactically correct SQL. If your syntax is incorrect
    /// you are likely to get a very unhelpful message from SqLite
    /// </summary>
    public interface IQueryBuilder
    {
        ISelectQueryBuilder Select();
        ISelectQueryBuilder SelectDistinct();
        ISelectColumnsBuilder Select(string columnName, params string[] columnNames);
        ISelectColumnsBuilder SelectDistinct(string columnName, params string[] columnNames);
        
        IDeleteQueryBuilder DeleteFrom(string tableName);
        
        IUpdateQueryBuilder Update(string tableName);
        IUpdateQueryBuilder UpdateOrRollback(string tableName);
        IUpdateQueryBuilder UpdateOrAbort(string tableName);
        IUpdateQueryBuilder UpdateOrReplace(string tableName);
        IUpdateQueryBuilder UpdateOrFail(string tableName);
        IUpdateQueryBuilder UpdateOrIgnore(string tableName);

        IInsertQueryBuilder InsertInto(string tableName, string columnName, params string[] columnNames);
        IInsertQueryBuilder InsertOrReplaceInto(string tableName, string columnName, params string[] columnNames);
        IInsertQueryBuilder InsertOrRollbackInto(string tableName, string columnName, params string[] columnNames);
        IInsertQueryBuilder InsertOrAbortInto(string tableName, string columnName, params string[] columnNames);
        IInsertQueryBuilder InsertOrFailInto(string tableName, string columnName, params string[] columnNames);
        IInsertQueryBuilder InsertOrIgnoreInto(string tableName, string columnName, params string[] columnNames);

        IInsertQueryBuilder ReplaceInto(string tableName, string columnName, params string[] columnNames);
    }
}
