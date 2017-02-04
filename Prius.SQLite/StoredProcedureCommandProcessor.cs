using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using Ioc.Modules;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.SqLite
{
    internal class StoredProcedureCommandProcessor: ICommandProcessor
    {
        public StoredProcedureCommandProcessor(
            IErrorReporter errorReporter,
            ICommand command, 
            SQLiteConnection connection, 
            SQLiteTransaction transaction)
        {
        }

        public int CommandTimeout
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public SQLiteParameterCollection Parameters
        {
            get { throw new NotImplementedException(); }
        }

        public Contracts.Interfaces.IDataReader ExecuteReader(string dataShapeName, Action<Contracts.Interfaces.IDataReader> closeAction, Action<Contracts.Interfaces.IDataReader> errorAction)
        {
            throw new NotImplementedException();
        }

        public long ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }


        public object ExecuteScalar()
        {
            throw new NotImplementedException();
        }
    }
}
