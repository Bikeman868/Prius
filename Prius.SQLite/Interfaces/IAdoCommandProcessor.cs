using System;
using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// This is for versions of ICommandProcessor that use the
    /// ADO.Net driver to talk to SQLite
    /// </summary>
    public interface IAdoCommandProcessor : ICommandProcessor
    {
        ICommandProcessor Initialize(
            IRepository repository,
            ICommand command,
            SQLiteConnection connection,
            SQLiteTransaction transaction);
    }
}
