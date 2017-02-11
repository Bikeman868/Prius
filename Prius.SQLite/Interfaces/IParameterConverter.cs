using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// Provides a mechanism for converting Prius IParameter interfaces
    /// into the SQLite equivalent.
    /// </summary>
    public interface IParameterConverter
    {
        void AddParameter(SQLiteCommand command, IParameter parameter);
    }
}
