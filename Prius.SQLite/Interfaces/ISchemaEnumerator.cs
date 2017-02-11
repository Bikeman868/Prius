using System;
using System.Collections.Generic;
using System.Reflection;
using Prius.SQLite.Schema;

namespace Prius.SQLite.Interfaces
{
    /// <summary>
    /// The schema enumerator is responsible for determining what database schema the
    /// application was compiled for. This can be uses to check if the database is
    /// compatible with the application, or to adjust the dtabase schema to match
    /// the application.
    /// </summary>
    public interface ISchemaEnumerator
    {
        void Clear();
        void ProbeBinFolderAssemblies();
        void Add(IEnumerable<Assembly> assemblies);
        void Add(Assembly assembly);

        IList<TableSchema> EnumerateTableSchemas();
    }
}
