using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prius.SqLite.QueryBuilder
{
    public interface IInsertQueryBuilder
    {
        IQuery Values(params string[] values);
    }
}
