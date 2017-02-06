namespace Prius.SqLite.QueryBuilder
{
    public interface IQueryBuilder
    {
        ISelectQueryBuilder Select(params string[] fields);
        ISelectQueryBuilder SelectDistinct(params string[] fields);
        IDeleteQueryBuilder DeleteFrom(string tableName);
        IUpdateQueryBuilder Update(string tableName);
        IInsertQueryBuilder InsertInto(string tableName, params string[] fields);
    }
}
