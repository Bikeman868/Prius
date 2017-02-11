using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SQLite.Procedures
{
    /// <summary>
    /// Objects of this type are passed to stored procedures that
    /// use the ADO.Net driver in System.Data.SQLite to talk to the
    /// SQLite database engine. It contains everything the stored
    /// procedure needs to execute and return an open data reader.
    /// </summary>
    public class AdoExecutionContext
    {
        public IList<IParameter> Parameters;
        public SQLiteConnection Connection;
        public SQLiteTransaction Transaction;
        public TextWriter MessageOutput;
        public string DataShapeName;
        public Action<IDataReader> CloseAction;
        public Action<IDataReader> ErrorAction;
        public int RowsAffected;
    }
}
