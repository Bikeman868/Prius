using System;
using Prius.Contracts.Interfaces;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;

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
