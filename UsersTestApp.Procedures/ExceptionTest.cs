using System;
using Prius.Contracts.Interfaces;
using Prius.SQLite.Interfaces;
using Prius.SQLite.Procedures;

namespace UsersTestApp.Procedures
{
    [Procedure("sp_ExceptionTest")]
    public class ExceptionTest : IAdoProcedure
    {
        public IDataReader Execute(AdoExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
