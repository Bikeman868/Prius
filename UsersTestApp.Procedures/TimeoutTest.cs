using Prius.Contracts.Interfaces;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;

namespace UsersTestApp.Procedures
{
    [Procedure("sp_TimeoutTest")]
    public class TimeoutTest : IAdoProcedure
    {
        public IDataReader Execute(AdoExecutionContext context)
        {
            while(true);
        }
    }
}
