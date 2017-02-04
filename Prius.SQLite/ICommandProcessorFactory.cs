using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite
{
    public interface ICommandProcessorFactory
    {
        ICommandProcessor Create(
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction);
    }
}
