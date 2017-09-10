using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Prius.Contracts.Exceptions;
using Prius.SQLite.Interfaces;
using Prius.Contracts.Interfaces.External;

namespace Prius.SQLite.Procedures
{
    /// <summary>
    /// This class implements IProcedureLibrary by reflecting over the DLLs
    /// in the bin folder of the application and finding classes that are
    /// decorated with the [Procedure] attribute and implement the IProcedure
    /// interface.
    /// Implements pooling and reusing of stored procedure implementations that
    /// are not thread safe.
    /// </summary>
    internal class ProcedureLibrary : IProcedureLibrary
    {
        private readonly IFactory _factory;
        private SortedList<string, Assembly> _probedAssemblies;
        private IList<ProcedureTypeWrapper> _procedureTypeWrappers;

        private const string TracePrefix = "Prius SQLite procedure library: ";

        public ProcedureLibrary(
            IFactory factory)
        {
            _factory = factory;

            Clear();
            ProbeBinFolderAssemblies();
        }

        public void Clear()
        {
            _procedureTypeWrappers = new List<ProcedureTypeWrapper>();
            _probedAssemblies = new SortedList<string, Assembly>();
        }

        public IProcedure Get(SQLiteConnection connection, string repositoryName, string procedureName)
        {
            var procedureTypeWrapper = _procedureTypeWrappers.FirstOrDefault(p =>
                (string.IsNullOrEmpty(p.Attribute.RepositoryName) ||
                 string.Equals(p.Attribute.RepositoryName, repositoryName, StringComparison.OrdinalIgnoreCase)
                ) && 
                string.Equals(p.Attribute.ProcedureName, procedureName, StringComparison.OrdinalIgnoreCase));

            if (procedureTypeWrapper == null)
                throw new PriusException("There is no procedure '" + procedureName + "' in repository '" + repositoryName + "'");

            return procedureTypeWrapper.Pool.Get();
        }

        public void Reuse(IProcedure procedure)
        {
            if (procedure != null)
            {
                var procedureType = _procedureTypeWrappers.FirstOrDefault(p => p.ProcedureType == procedure.GetType());
                if (procedureType != null)
                    procedureType.Pool.Reuse(procedure);
            }
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
                            var procedureTypeWrapper = new ProcedureTypeWrapper
                            {
                                Attribute = attribute,
                                ProcedureType = type,
                                Pool = attribute.IsThreadSafe 
                                    ? new SingleInstanceProcedurePool(_factory, type) as ProcedurePool
                                    : new MultiInstanceProcedurePool(_factory, type)
                            };
                            _procedureTypeWrappers.Add(procedureTypeWrapper);
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
                throw new PriusException(TracePrefix + "Unable to discover the path to the bin folder");

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

        private class ProcedureTypeWrapper
        {
            public Type ProcedureType;
            public ProcedureAttribute Attribute;
            public ProcedurePool Pool; 
        }

        private abstract class ProcedurePool
        {
            public abstract IProcedure Get();
            public abstract void Reuse(IProcedure procedure);
        }

        private class SingleInstanceProcedurePool: ProcedurePool
        {
            private readonly IProcedure _instance;

            public SingleInstanceProcedurePool(IFactory factory, Type type)
            {
                _instance = factory.Create(type) as IProcedure;
            }

            public override IProcedure Get()
            {
                return _instance;
            }

            public override void Reuse(IProcedure procedure)
            {
            }
        }

        private class MultiInstanceProcedurePool : ProcedurePool
        {
            private readonly IFactory _factory;
            private readonly Type _type;
            private readonly Queue<IProcedure> _pool;

            public MultiInstanceProcedurePool(IFactory factory, Type type)
            {
                _factory = factory;
                _type = type;
                _pool = new Queue<IProcedure>();
            }

            public override IProcedure Get()
            {
                lock(_pool)
                {
                    if (_pool.Count > 0)
                        return _pool.Dequeue();
                }
                
                return _factory.Create(_type) as IProcedure;
            }

            public override void Reuse(IProcedure procedure)
            {
                if (procedure != null)
                    lock (_pool) _pool.Enqueue(procedure);
            }
        }
    }

}
