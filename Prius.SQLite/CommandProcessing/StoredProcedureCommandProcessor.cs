using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
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

        public int CommandTimeout { get; set; }

        public SQLiteParameterCollection Parameters
        {
            get { throw new NotImplementedException(); }
        }

        public IDataReader ExecuteReader(string dataShapeName, Action<IDataReader> closeAction, Action<IDataReader> errorAction)
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
