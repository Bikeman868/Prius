using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.Schema
{
    /// <summary>
    /// Reflects over DLLs in the bin folder and finds classes that
    /// are decorated with schema related attributes. Builds a complete
    /// definition of what the SqLite database schema should be like to
    /// work with the application code.
    /// </summary>
    internal class SchemaEnumerator: ISchemaEnumerator
    {
        private readonly IColumnTypeMapper _columnTypeMapper;
        private SortedList<string, Assembly> _probedAssemblies;
        private IList<TableSchema> _tables;

        private const string TracePrefix = "Prius SqLite schema enumerator: ";

        public SchemaEnumerator(IColumnTypeMapper columnTypeMapper)
        {
            _columnTypeMapper = columnTypeMapper;

            Clear();
            ProbeBinFolderAssemblies();
        }

        public void Clear()
        {
            _tables = new List<TableSchema>();
            _probedAssemblies = new SortedList<string, Assembly>();
        }

        public IList<TableSchema> EnumerateTableSchemas()
        {
            return _tables;
        }

        public void Add(Assembly assembly)
        {
            if (!_probedAssemblies.ContainsKey(assembly.FullName))
                _probedAssemblies.Add(assembly.FullName, assembly);

            foreach (var type in assembly.GetTypes())
            {
                try
                {
                    foreach (SchemaTableAttribute connectionProviderAttribute in type.GetCustomAttributes(typeof(SchemaTableAttribute), true))
                    {
                        var tableSchema = new TableSchema
                        {
                            RepositoryName = connectionProviderAttribute.RepositoryName,
                            TableName = connectionProviderAttribute.TableName,
                            Columns = new List<ColumnSchema>(),
                            Indexes = new List<IndexSchema>()
                        };
                        _tables.Add(tableSchema);

                        foreach (var propertyInfo in type.GetProperties())
                        {
                            string columnName = null;
                            foreach (SchemaColumnAttribute column in propertyInfo.GetCustomAttributes(typeof(SchemaColumnAttribute), true))
                            {
                                columnName = column.ColumnName;
                                tableSchema.Columns.Add(new ColumnSchema
                                {
                                    ColumnName = column.ColumnName,
                                    DataType = _columnTypeMapper.MapToSqLite(column.DataType),
                                    Attributes = column.ColumnAttributes
                                });
                            }
                            foreach (SchemaIndexAttribute index in propertyInfo.GetCustomAttributes(typeof(SchemaIndexAttribute), true))
                            {
                                if (columnName == null)
                                    throw new Exception(
                                        "You can not add an index to a property that is not mapped to a column in the database. "+
                                        "Please check the '" + propertyInfo.Name + "' property of the '" + type.FullName + "' class.");

                                var schemaIndex = tableSchema.Indexes.FirstOrDefault(i => string.Equals(i.IndexName, index.IndexName, StringComparison.OrdinalIgnoreCase));
                                if (schemaIndex == null)
                                {
                                    schemaIndex = new IndexSchema
                                    {
                                        IndexName = index.IndexName,
                                        Attributes = index.IndexAttributes,
                                        ColumnNames = new [] {columnName}
                                    };
                                    tableSchema.Indexes.Add(schemaIndex);
                                }
                                else
                                {
                                    schemaIndex.ColumnNames = schemaIndex.ColumnNames.Concat(new[] {columnName}).ToArray();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Exception whilst examining type " + type.FullName
                              + " in assembly " + assembly.FullName
                              + ". " + ex.Message;
                    Trace.WriteLine(TracePrefix + msg);
                }
            }
        }

        public void Add(IEnumerable<Assembly> assemblies)
        {
            foreach (var assembly in assemblies)
            {
                if (!_probedAssemblies.ContainsKey(assembly.FullName))
                {
                    try
                    {
                        Add(assembly);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        var msg = "Exception probiing assembly " + assembly.FullName + ". " + ex.Message;
                        Trace.WriteLine(TracePrefix + msg);
                        if (ex.LoaderExceptions != null)
                        {
                            foreach (var loaderException in ex.LoaderExceptions)
                            {
                                var loaderMsg = "  Loader exception " + loaderException.Message;
                                Trace.WriteLine(loaderMsg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var msg = "Exception probiing assembly " + assembly.FullName + ". " + ex.Message;
                        Trace.WriteLine(TracePrefix + msg);
                    }
                }
            }
        }

        public void ProbeBinFolderAssemblies()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var assemblyUri = new UriBuilder(codeBase);
            var assemblyPath = Uri.UnescapeDataString(assemblyUri.Path);
            var binFolderPath = Path.GetDirectoryName(assemblyPath);

            if (ReferenceEquals(binFolderPath, null))
                throw new Exception(TracePrefix + "Unable to discover the path to the bin folder");

            var assemblyFileNames = Directory.GetFiles(binFolderPath, "*.dll");

            var assemblies = assemblyFileNames
                .Select(fileName => new AssemblyName(Path.GetFileNameWithoutExtension(fileName)))
                .Select(name =>
                {
                    try
                    {
                        Trace.WriteLine(TracePrefix + "Probing bin folder assembly " + name);
                        return AppDomain.CurrentDomain.Load(name);
                    }
                    catch (FileNotFoundException ex)
                    {
                        var msg = "File not found exception loading " + name + ". " + ex.FileName;
                        Trace.WriteLine(TracePrefix + msg);
                        return null;
                    }
                    catch (BadImageFormatException ex)
                    {
                        var msg = "Bad image format exception loading " + name + ". The DLL is probably not a .Net assembly. " + ex.FusionLog;
                        Trace.WriteLine(TracePrefix + msg);
                        return null;
                    }
                    catch (FileLoadException ex)
                    {
                        var msg = "File load exception loading " + name + ". " + ex.FusionLog;
                        Trace.WriteLine(TracePrefix + msg);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        var msg = "Failed to load assembly " + name + ". " + ex.Message;
                        Trace.WriteLine(TracePrefix + msg);
                        return null;
                    }
                })
                .Where(a => a != null);
            Add(assemblies);
        }
    }
}
