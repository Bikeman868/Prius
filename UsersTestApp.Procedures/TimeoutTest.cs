using Prius.Contracts.Interfaces;
using Prius.SQLite.Interfaces;
using Prius.SQLite.Procedures;

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
