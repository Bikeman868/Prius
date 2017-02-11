namespace Prius.SQLite.QueryBuilder
{
    public interface IInsertQueryBuilder
    {
        IQuery DefaultValues();
        IQuery Values(params string[] values);
        ISelectQueryBuilder Select();
        ISelectQueryBuilder SelectDistinct();
        ISelectColumnsBuilder Select(string columnName, params string[] columnNames);
        ISelectColumnsBuilder SelectDistinct(string columnName, params string[] columnNames);
    }
}
