using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Prius.SqLite.Interfaces;
using Prius.Contracts.Interfaces.External;
using Prius.SqLite.Schema;

namespace Prius.SqLite.Procedures
{
    internal class ProcedureLibrary : IProcedureLibrary
    {
        private readonly IFactory _factory;
        private SortedList<string, Assembly> _probedAssemblies;
        private IList<Procedure> _procedures;

        private const string TracePrefix = "Prius SqLite procedure library: ";

        public ProcedureLibrary(
            IFactory factory)
        {
            _factory = factory;

            Clear();
            ProbeBinFolderAssemblies();
        }

        public void Clear()
        {
            _procedures = new List<Procedure>();
            _probedAssemblies = new SortedList<string, Assembly>();
        }

        public IProcedure Get(SQLiteConnection connection, string repositoryName, string procedureName)
        {
            var procedure = _procedures.FirstOrDefault(p =>
                (string.IsNullOrEmpty(p.Attribute.RepositoryName) ||
                 string.Equals(p.Attribute.RepositoryName, repositoryName, StringComparison.OrdinalIgnoreCase)
                ) && 
                string.Equals(p.Attribute.ProcedureName, procedureName, StringComparison.OrdinalIgnoreCase));

            if (procedure == null) return null;

            if (procedure.Instance == null)
            {
                lock(procedure)
                {
                    if (procedure.Instance == null)
                    {
                        procedure.Instance = _factory.Create(procedure.ProcedureType) as IProcedure;
                    }
                }
            }

            // TODO: Check is procedure is thread safe and create a pool if not

            return procedure.Instance;
        }

        public void Reuse(IProcedure procedure)
        {
            // TODO: pool and reuse non-thread safe procedures
        }

        public void Add(Assembly assembly)
        {
            if (!_probedAssemblies.ContainsKey(assembly.FullName))
                _probedAssemblies.Add(assembly.FullName, assembly);

            var procedureInterface = typeof(IProcedure);
            foreach (var type in assembly.GetTypes())
            {
                try
                {
                    if (procedureInterface.IsAssignableFrom(type))
                    {
                        foreach (ProcedureAttribute attribute in type.GetCustomAttributes(typeof(ProcedureAttribute), true))
                        {
                            var procedure = new Procedure
                            {
                                Attribute = attribute,
                                ProcedureType = type
                            };
                            _procedures.Add(procedure);
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

        private class Procedure
        {
            public Type ProcedureType;
            public ProcedureAttribute Attribute;
            public IProcedure Instance; 
        }
    }

}
