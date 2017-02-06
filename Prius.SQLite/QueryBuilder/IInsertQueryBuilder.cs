namespace Prius.SqLite.QueryBuilder
{
    public interface IInsertQueryBuilder
    {
        IQuery DefaultValues();
        IQuery Values(params string[] values);
        ISelectQueryBuilder Select(params string[] fields);
        ISelectQueryBuilder SelectDistinct(params string[] fields);
    }
}
