using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.SqLite.Interfaces
{
    public interface ICommandProcessorFactory
    {
        ICommandProcessor Create(
            IRepository repository,
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction);
    }
}
