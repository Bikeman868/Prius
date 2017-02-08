namespace Prius.SqLite.QueryBuilder
{
    /// <summary>
    /// Understands basic SqLite SQL syntax and assists developers in
    /// writing syntactically correct SQL. If your syntax is incorrect
    /// you are likely to get a very unhelpful message from SqLite
    /// </summary>
    public interface IQueryBuilder
    {
        ISelectQueryBuilder Select(params string[] fields);
        ISelectQueryBuilder SelectDistinct(params string[] fields);
        
        IDeleteQueryBuilder DeleteFrom(string tableName);
        
        IUpdateQueryBuilder Update(string tableName);
        IUpdateQueryBuilder UpdateOrRollback(string tableName);
        IUpdateQueryBuilder UpdateOrAbort(string tableName);
        IUpdateQueryBuilder UpdateOrReplace(string tableName);
        IUpdateQueryBuilder UpdateOrFail(string tableName);
        IUpdateQueryBuilder UpdateOrIgnore(string tableName);

        IInsertQueryBuilder InsertInto(string tableName, params string[] fields);
        IInsertQueryBuilder InsertOrReplaceInto(string tableName, params string[] fields);
        IInsertQueryBuilder InsertOrRollbackInto(string tableName, params string[] fields);
        IInsertQueryBuilder InsertOrAbortInto(string tableName, params string[] fields);
        IInsertQueryBuilder InsertOrFailInto(string tableName, params string[] fields);
        IInsertQueryBuilder InsertOrIgnoreInto(string tableName, params string[] fields);

        IInsertQueryBuilder ReplaceInto(string tableName, params string[] fields);
    }
}
