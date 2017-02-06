using System.Collections.Generic;
using System.Data.SQLite;
using System.Reflection;

namespace Prius.SqLite.Interfaces
{
    public interface IProcedureLibrary
    {
        void ProbeBinFolderAssemblies();
        void Add(IEnumerable<Assembly> assemblies);
        void Add(Assembly assembly);

        IProcedure Get(SQLiteConnection connection, string repositoryName, string procedureName);
        void Reuse(IProcedure procedure);
    }
}
