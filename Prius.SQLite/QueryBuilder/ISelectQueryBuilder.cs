namespace Prius.SQLite.QueryBuilder
{
    public interface ISelectQueryBuilder : IQuery
    {
        ISelectColumnsBuilder Column(string columnName, string alias = null);
        ISelectColumnsBuilder SubQuery(IQuery subquery, string alias = null);
    }

    public interface ISelectColumnsBuilder : IQuery
    {
        ISelectColumnsBuilder Column(string columnName, string alias = null);
        ISelectColumnsBuilder SubQuery(IQuery subquery, string alias = null);
        ISelectFromQueryBuilder From(string tableName, string alias = null);
    }

    public interface ISelectFromQueryBuilder : IQuery
    {
        ISelectFromQueryBuilder Join(string leftTable, string leftColumn, string rightTable, string rightColumn);
        ISelectFromQueryBuilder Join(string leftTable, string columnName, string rightTable);
        ISelectFromQueryBuilder LeftJoin(string leftTable, string leftColumn, string rightTable, string rightColumn);
        ISelectWhereQueryBuilder Where(string condition);
        ISelectOrderQueryBuilder OrderBy(string columnName, params string[] columnNames);
        ISelectGroupQueryBuilder GroupBy(string columnName, params string[] columnNames);
    }

    public interface ISelectWhereQueryBuilder : IQuery
    {
        ISelectWhereQueryBuilder And(string condition);
        ISelectOrderQueryBuilder OrderBy(string columnName, params string[] columnNames);
    }

    public interface ISelectGroupQueryBuilder: IQuery
    {
        ISelectOrderQueryBuilder Having(string expression);
        ISelectOrderQueryBuilder OrderBy(string columnName, params string[] columnNames);
    }

    public interface ISelectOrderQueryBuilder : IQuery
    {
        ISelectOrderCollateQueryBuilder Collate(string collation);
        ISelectOrderQueryBuilder Ascending();
        ISelectOrderQueryBuilder Descending();
        ISelectOrderQueryBuilder ThenBy(string columnName);
        IQuery Limit(int rowCount);
    }

    public interface ISelectOrderCollateQueryBuilder : IQuery
    {
        ISelectOrderQueryBuilder Ascending();
        ISelectOrderQueryBuilder Descending();
        ISelectOrderQueryBuilder ThenBy(string columnName);
        IQuery Limit(int rowCount);
    }
}
