using System.Collections.Generic;
using System.Data.SQLite;
using System.Reflection;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// Encapsulates the library of all of the stored procedures. The stored
    /// procedure command processor will go to the library to get a stored
    /// procedure, then put it back into the library for reuse when the
    /// stored procedure execution is complete.
    /// </summary>
    public interface IProcedureLibrary
    {
        void ProbeBinFolderAssemblies();
        void Add(IEnumerable<Assembly> assemblies);
        void Add(Assembly assembly);

        IProcedure Get(SQLiteConnection connection, string repositoryName, string procedureName);
        void Reuse(IProcedure procedure);
    }
}
