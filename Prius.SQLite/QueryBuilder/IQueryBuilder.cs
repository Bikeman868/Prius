namespace Prius.SqLite.QueryBuilder
{
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
