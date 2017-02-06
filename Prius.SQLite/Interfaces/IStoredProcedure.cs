using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    public interface IStoredProcedure
    {
        IDataReader Execute(IList<IParameter> parameters, SQLiteConnection connection, SQLiteTransaction transaction, TextWriter messageOutput);
    }
}
