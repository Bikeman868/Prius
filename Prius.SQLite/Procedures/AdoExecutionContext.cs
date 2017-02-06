using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Procedures
{
    public class AdoExecutionContext
    {
        public IList<IParameter> Parameters;
        public SQLiteConnection Connection;
        public SQLiteTransaction Transaction;
        public TextWriter MessageOutput;
        public string DataShapeName;
        public Action<IDataReader> CloseAction;
        public Action<IDataReader> ErrorAction;
    }
}
