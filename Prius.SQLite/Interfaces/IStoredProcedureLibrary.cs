using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Prius.SqLite.Interfaces
{
    public interface IStoredProcedureLibrary
    {
        IStoredProcedure GetProcedure(SQLiteConnection connection, string procedureName);
    }
}
