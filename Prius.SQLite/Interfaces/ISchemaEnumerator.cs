using System;
using System.Collections.Generic;
using System.Reflection;
using Prius.SqLite.Schema;

namespace Prius.SqLite.Interfaces
{
    public interface ISchemaEnumerator
    {
        void Clear();
        void ProbeBinFolderAssemblies();
        void Add(IEnumerable<Assembly> assemblies);
        void Add(Assembly assembly);

        IList<TableSchema> EnumerateTableSchemas();
    }
}
