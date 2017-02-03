using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;


namespace Prius.Orm.Connections
{
    public class ConnectionFactory: IConnectionFactory
    {
        private readonly IFactory _factory;

        private IDictionary<string, Type> _providers;
        private SortedList<string, Assembly> _probedAssemblies;

        private const string _tracePrefix = "Prius connection factory: ";
        
        public ConnectionFactory(
            IFactory factory)
        {
            _factory = factory;
            Clear();
        }

        public void Clear()
        {
            _providers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _probedAssemblies = new SortedList<string, Assembly>();

            ProbeBinFolderAssemblies();
        }

        public IConnection Create(string serverType, IRepository repository, ICommand command, string connectionString, string schema)
        {
            var type = GetProvider(serverType);
            var provider = ConstructProvider(type);
            return provider.Open(repository, command, connectionString, schema);
        }

        private Type GetProvider(string serverType)
        {
            Type type;
            lock (_providers)
                if (_providers.TryGetValue(serverType, out type))
                    return type;

            throw new ApplicationException("No registered provider for '" + 
                serverType + "'. Please install the NuGet package that provides "+
                "access to this type of server.");
        }

        private IConnectionProvider ConstructProvider(Type type)
        {
            var provider = _factory.Create(type) as IConnectionProvider;
            return provider;
        }

        private void LoadProviders()
        {

        }

        private void Add(Assembly assembly)
        {
            if (!_probedAssemblies.ContainsKey(assembly.FullName))
                _probedAssemblies.Add(assembly.FullName, assembly);

            var connectionProviderInterface = typeof(IConnectionProvider);
            foreach (var type in assembly.GetTypes())
            {
                try
                {
                    foreach(ProviderAttribute connectionProviderAttribute in type.GetCustomAttributes(typeof(ProviderAttribute), true))
                    {
                        if (connectionProviderInterface.IsAssignableFrom(type))
                        {
                            if (!_providers.ContainsKey(connectionProviderAttribute.ServerType))
                                _providers.Add(connectionProviderAttribute.ServerType, type);
                        }
                        else
                        {
                            var msg = "Type " + type.FullName +
                                      " in assembly " + assembly.FullName +
                                      " has the [ProviderAttribute] attribute but does not implement the IConnectionProvider interface.";
                            Trace.WriteLine(_tracePrefix + msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var msg = "Exception whilst examining type " + type.FullName
                              + " in assembly " + assembly.FullName
                              + ". " + ex.Message;
                    Trace.WriteLine(_tracePrefix + msg);
                }
            }
        }

        private void Add(IEnumerable<Assembly> assemblies)
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
                        Trace.WriteLine(_tracePrefix + msg);
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
                        Trace.WriteLine(_tracePrefix + msg);
                    }
                }
            }
        }

        private void ProbeBinFolderAssemblies()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var assemblyUri = new UriBuilder(codeBase);
            var assemblyPath = Uri.UnescapeDataString(assemblyUri.Path);
            var binFolderPath = Path.GetDirectoryName(assemblyPath);

            if (ReferenceEquals(binFolderPath, null))
                throw new Exception(_tracePrefix + "Unable to discover the path to the bin folder");

            var assemblyFileNames = Directory.GetFiles(binFolderPath, "*.dll");

            var assemblies = assemblyFileNames
                .Select(fileName => new AssemblyName(Path.GetFileNameWithoutExtension(fileName)))
                .Select(name =>
                {
                    try
                    {
                        Trace.WriteLine(_tracePrefix + "Probing bin folder assembly " + name);
                        return AppDomain.CurrentDomain.Load(name);
                    }
                    catch (FileNotFoundException ex)
                    {
                        var msg = "File not found exception loading " + name + ". " + ex.FileName;
                        Trace.WriteLine(_tracePrefix + msg);
                        return null;
                    }
                    catch (BadImageFormatException ex)
                    {
                        var msg = "Bad image format exception loading " + name + ". The DLL is probably not a .Net assembly. " + ex.FusionLog;
                        Trace.WriteLine(_tracePrefix + msg);
                        return null;
                    }
                    catch (FileLoadException ex)
                    {
                        var msg = "File load exception loading " + name + ". " + ex.FusionLog;
                        Trace.WriteLine(_tracePrefix + msg);
                        return null;
                    }
                    catch (Exception ex)
                    {
                        var msg = "Failed to load assembly " + name + ". " + ex.Message;
                        Trace.WriteLine(_tracePrefix + msg);
                        return null;
                    }
                })
                .Where(a => a != null);
            Add(assemblies);
        }
    }
}
