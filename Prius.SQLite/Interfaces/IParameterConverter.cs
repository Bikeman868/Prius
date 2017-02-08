using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    /// <summary>
    /// Provides a mechanism for converting Prius IParameter interfaces
    /// into the SqLite equivalent.
    /// </summary>
    public interface IParameterConverter
    {
        void AddParameter(SQLiteCommand command, IParameter parameter);
    }
}
