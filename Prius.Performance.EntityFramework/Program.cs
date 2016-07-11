using Ioc.Modules;
using Microsoft.Practices.Unity;
using Prius.Performance.Shared;
using System;
using System.IO;
using System.Reflection;

namespace Prius.Performance.EntityFramework
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get our Entity Framework implementation of the data access layer
            var dal = new DataAccessLayer();

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
            container.RegisterInstance(container);

            var packageLocator = new PackageLocator().ProbeBinFolderAssemblies().Add(Assembly.GetExecutingAssembly());
            Ioc.Modules.Unity.Registrar.Register(packageLocator, container);

            return container;
        }
    }
}
