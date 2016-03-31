using Prius.Contracts.Enumerations;
using Prius.Contracts.Interfaces;

namespace Prius.Orm.Commands
{
    public class CommandFactory: ICommandFactory
    {
        private readonly IParameterFactory _parameterFactory;

        public CommandFactory(IParameterFactory parameterFactory)
        {
            _parameterFactory = parameterFactory;
        }

        public ICommand CreateStoredProcedure(string procedureName, int? timeoutSeconds)
        {
            return new Command(_parameterFactory).Initialize(CommandType.StoredProcedure, procedureName, timeoutSeconds);
        }

        public ICommand CreateSql(string sql, int? timeoutSeconds)
        {
            return new Command(_parameterFactory).Initialize(CommandType.SQL, sql, timeoutSeconds);
        }
    }
}
