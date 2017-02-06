using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.StoredProcedures
{
    internal class StoredProcedureRunner : IStoredProcedureRunner
    {
        public IDataReader ExecuteReader(IStoredProcedure storedProcedure, ICommand command, int commandTimeout, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public object ExecuteScalar(IStoredProcedure storedProcedure, ICommand command, int commandTimeout, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            throw new NotImplementedException();
        }

        public long ExecuteNonQuery(IStoredProcedure storedProcedure, ICommand command, int commandTimeout, SQLiteConnection connection, SQLiteTransaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}
