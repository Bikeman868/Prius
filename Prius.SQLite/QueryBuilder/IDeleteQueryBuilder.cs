using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.SqLite.QueryBuilder
{
    public interface IDeleteQueryBuilder: IQuery
    {
        IDeleteWhereQueryBuilder Where(string condition);
    }

    public interface IDeleteWhereQueryBuilder : IQuery
    {
        IDeleteWhereQueryBuilder And(string condition);
    }
}
