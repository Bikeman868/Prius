using System;
using System.IO;
using Microsoft.Practices.Unity;
using Prius.Contracts.Interfaces;
using Prius.Orm.Commands;
using Prius.Orm.Connections;
using Prius.Orm.Enumeration;
using Prius.Orm.Results;
using Prius.Performance.Prius.Integration;
using Prius.Performance.Prius.Model;
using Prius.Performance.Shared;
using Urchin.Client.Data;
using Urchin.Client.Interfaces;
using Urchin.Client.Sources;

namespace Prius.Performance.Prius
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ConfigureIoc();

            // Load the urchin config file. Prius uses Urchin for configuration
            var urchinConfigFile = container.Resolve<FileSource>();
            urchinConfigFile.Initialize(new FileInfo("urchin.json"), TimeSpan.FromSeconds(10));

            // Get our Prius implementation of the data access layer
            var dal = container.Resolve<IDataAccessLayer>();

            // Get the shared performance test runner
            var test = new PerformanceTest(dal);

            // Run all the performance tests and output results to the console
            using (var output = Console.OpenStandardOutput())
            {
                using (var writer = new StreamWriter(output))
                {
                    test.RunTests(writer);
                }
            }
            Console.ReadKey();
        }

        private static UnityContainer ConfigureIoc()
        {
            var container = new UnityContainer();

            // Register application classes
            container.RegisterType<IDataAccessLayer, DataAccessLayer>(new ContainerControlledLifetimeManager());
            container.RegisterType<ICustomer, Customer>(new TransientLifetimeManager());
            container.RegisterType<IOrder, Order>(new TransientLifetimeManager());

            // Register application provided interfaces that Prius depends on
            container.RegisterInstance<IFactory>(new Factory(container));
            container.RegisterType<IErrorReporter, ErrorReporter>(new ContainerControlledLifetimeManager());

            // Register Urchin configuration management system
            container.RegisterType<IConfigurationStore, ConfigurationStore>(new ContainerControlledLifetimeManager());
             
            // Register classes defined within the Prius library
            container.RegisterType<ICommandFactory, CommandFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IConnectionFactory, ConnectionFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContextFactory, ContextFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataEnumeratorFactory, DataEnumeratorFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDataReaderFactory, DataReaderFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IMapper, Mapper>(new ContainerControlledLifetimeManager());
            container.RegisterType<IParameterFactory, ParameterFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IRepositoryFactory, RepositoryFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IEnumerableDataFactory, EnumerableDataFactory>(new ContainerControlledLifetimeManager());
            container.RegisterType<IAsyncEnumerableFactory, AsyncEnumerableFactory>(new ContainerControlledLifetimeManager());

            return container;
        }
    }
}
