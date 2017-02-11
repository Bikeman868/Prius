using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.SQLite.QueryBuilder
{
    public interface IUpdateQueryBuilder
    {
        IUpdateSetQueryBuilder Set(string field, string value);
    }

    public interface IUpdateSetQueryBuilder
    {
        IUpdateSetQueryBuilder Set(string field, string value);
        IUpdateSetQueryBuilder Where(string condition);
    }

    public interface IUpdateWhereQueryBuilder : IQuery
    {
        IUpdateWhereQueryBuilder And(string condition);
    }
}
