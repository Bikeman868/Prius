using Prius.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Prius.SqLite
{
    public interface ICommandProcessor
    {
        int CommandTimeout { get; set; }
        SQLiteParameterCollection Parameters { get; }

        IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction);
        long ExecuteNonQuery();
        object ExecuteScalar();
    }
}
