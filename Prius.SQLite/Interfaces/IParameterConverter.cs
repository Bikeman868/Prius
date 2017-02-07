using System.Data.SQLite;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    public interface IParameterConverter
    {
        void AddParameter(SQLiteCommand command, IParameter parameter);
    }
}
