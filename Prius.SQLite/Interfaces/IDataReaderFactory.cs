using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Prius.Contracts.Interfaces;

namespace Prius.SqLite.Interfaces
{
    public interface IDataReaderFactory
    {
        IDataReader Create(SQLiteDataReader sqLiteDataReader);
    }
}
