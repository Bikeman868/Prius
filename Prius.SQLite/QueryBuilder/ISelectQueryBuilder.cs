using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.SqLite.QueryBuilder
{
    public interface ISelectQueryBuilder : IQuery
    {
        ISelectFromQueryBuilder From(string tableName);
    }

    public interface ISelectFromQueryBuilder : IQuery
    {
        ISelectFromQueryBuilder Join(string leftTable, string leftField, string rightTable, string rightField);
        ISelectFromQueryBuilder LeftJoin(string leftTable, string leftField, string rightTable, string rightField);
        ISelectWhereQueryBuilder Where(string condition);
        ISelectOrderQueryBuilder OrderBy(params string[] fieldNames);
        ISelectGroupQueryBuilder GroupBy(params string[] fieldNames);
    }

    public interface ISelectWhereQueryBuilder : IQuery
    {
        ISelectWhereQueryBuilder And(string condition);
        ISelectOrderQueryBuilder OrderBy(params string[] fieldNames);
    }

    public interface ISelectGroupQueryBuilder: IQuery
    {
        ISelectOrderQueryBuilder Having(string expression);
        ISelectOrderQueryBuilder OrderBy(params string[] fieldNames);
    }

    public interface ISelectOrderQueryBuilder : IQuery
    {
        ISelectOrderCollateQueryBuilder Collate(string collation);
        ISelectOrderQueryBuilder Ascending();
        ISelectOrderQueryBuilder Descending();
        ISelectOrderQueryBuilder ThenBy(string fieldName);
        IQuery Limit(int rowCount);
    }

    public interface ISelectOrderCollateQueryBuilder : IQuery
    {
        ISelectOrderQueryBuilder Ascending();
        ISelectOrderQueryBuilder Descending();
        ISelectOrderQueryBuilder ThenBy(string fieldName);
        IQuery Limit(int rowCount);
    }
}
