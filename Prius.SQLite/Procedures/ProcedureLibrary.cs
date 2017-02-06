using System;
using System.Data.SQLite;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.Procedures
{
    internal class ProcedureLibrary : IProcedureLibrary
    {
        public IProcedure Get(SQLiteConnection connection, string procedureName)
        {
            throw new NotImplementedException();
        }

        public void Reuse(IProcedure procedure)
        {
            throw new NotImplementedException();
        }
    }
}
