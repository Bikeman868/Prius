using System;
using System.Data.SQLite;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.StoredProcedures
{
    internal class StoredProcedureLibrary : IStoredProcedureLibrary
    {
        public IStoredProcedure GetProcedure(SQLiteConnection connection, string procedureName)
        {
            throw new NotImplementedException();
        }
    }
}
