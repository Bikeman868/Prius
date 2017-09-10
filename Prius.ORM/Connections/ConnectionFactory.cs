using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Prius.Contracts.Attributes;
using Prius.Contracts.Exceptions;
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

        private const string TracePrefix = "Prius connection factory: ";
        
        public ConnectionFactory(
            IFactory factory)
        {
            _factory = factory;
            Clear();
            ProbeBinFolderAssemblies();
        }

        public void Clear()
        {
            _providers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _probedAssemblies = new SortedList<string, Assembly>();
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

            throw new PriusException("Your database config specifies a server type of '" + 
                serverType + "' but there are no installed Prius drivers for this type of server. "+
                "Please install the Prius NuGet package that provides access to " + serverType + 
                " databases or correct your config. If you are not sure what to do, "+
                "try opening the Package Manager Console and type 'Install-Package "+
                "Prius." + serverType + "'");
        }

        private IConnectionProvider ConstructProvider(Type type)
        {
            var provider = _factory.Create(type) as IConnectionProvider;
            return provider;
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
                            {
                                _providers.Add(connectionProviderAttribute.ServerType, type);
                                var msg = "Found '" + connectionProviderAttribute.ServerType +
                                          "' provider " + type.FullName + " in assembly " + 
                                          assembly.GetName().Name + ". " ;
                                Trace.WriteLine(TracePrefix + msg);
                            }
                        }
                        else
                        {
                            var msg = "Type " + type.FullName +
                                      " in assembly " + assembly.FullName +
                                      " has the [ProviderAttribute] attribute but does not implement the IConnectionProvider interface.";
                            Trace.WriteLine(TracePrefix + msg);
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

        private void ProbeBinFolderAssemblies()
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
    }
}
