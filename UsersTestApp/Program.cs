using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Ioc.Modules;
using Microsoft.Practices.Unity;
using Prius.Contracts.Interfaces;
using Urchin.Client.Sources;
using Prius.Contracts.Interfaces.External;
using UsersTestApp.DataAccess;

namespace UsersTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Initialize();

            Help();

            if (args != null && args.Length > 0)
                Use(args[0]);

            var quit = false;
            while (!quit)
            {
                try
                {
                    Console.Write("> ");
                    args = Console.ReadLine().Split(' ');
                    if (args.Length > 0)
                    {
                        var cmd = args[0].ToLower();
                        args = args.Skip(1).ToArray();
                        if (cmd == "help") Help(args);
                        else if (cmd == "use") Use(args);
                        else if (cmd == "add") Add(args);
                        else if (cmd == "list") List(args);
                        else if (cmd == "delete") Delete(args);
                        else if (cmd == "exit" || cmd =="quit") quit = true;
                    }
                }
                catch (Exception ex)
                {
                    while (ex != null)
                    {
                        Console.WriteLine(ex.Message);
                        ex = ex.InnerException;
                    }
                    Console.WriteLine();
                    Console.WriteLine("Type the command 'help' to see available command syntax");
                }
            }
        }

        private static IErrorReporter _errorReporter;
        private static IDataAccessLayer _dataAccessLayer;

        private static string _repository;

        private static void Use(params string[] args)
        {
            if (args.Length < 1) args = new[] {"SqLite"};
            _repository = args[0];
            Console.WriteLine("Using repository " + args[0]);
        }

        private static void List(params string[] args)
        {
            var users = _dataAccessLayer.GetUsers(_repository);
            if (users == null)
            {
                Console.WriteLine("No user list returned");
            }
            else
            {
                Console.WriteLine(string.Format("{0,10} {1,15} {2,20} {3,10}", "Id", "Fist name", "Last name", "Age"));
                foreach (var user in users)
                {
                    var age = user.DateOfBirth == DateTime.MinValue ? "" : ((DateTime.UtcNow - user.DateOfBirth).TotalDays / 365).ToString();
                    Console.WriteLine(string.Format("{0,10} {1,15} {2,20} {3,10}", user.UserId, user.FirstName, user.LastName, age));
                }
            }
        }

        private static void Add(params string[] args)
        {
            var user = _dataAccessLayer.AddUser(_repository, args[0], args[1]);
            if (user == null)
                Console.WriteLine("Failed to add user");
            else
            Console.WriteLine("Added user with id " + user.UserId);

            if (args.Length > 2)
                ; // TODO: Update DOB
        }

        private static void Delete(params string[] args)
        {
            var success = _dataAccessLayer.DeleteUser(_repository, int.Parse(args[0]));
            Console.WriteLine(success ? "Succesfully deleted user" : "User not found");
        }

        private static void Help(params string[] args)
        {
            Console.WriteLine("Prius Test Application.");
            Console.WriteLine("");
            Console.WriteLine("This very simple demo allows you to add, delete and list users");
            Console.WriteLine("in a single database table. The app supports all of the different");
            Console.WriteLine("database drivers, so you can switch between SQL Server, MySQL and SQLite");
            Console.WriteLine("for example without recompiling the code.");
            Console.WriteLine("");
            Console.WriteLine("Type one of these commands and enter:");
            Console.WriteLine("");
            Console.WriteLine("  use <repository>");
            Console.WriteLine("  add <firstName> <lastName> [<dob>]");
            Console.WriteLine("  list");
            Console.WriteLine("  delete <userId>");
            Console.WriteLine("  close");
            Console.WriteLine("  exit");
            Console.WriteLine("  help");
            Console.WriteLine("");
        }

        #region Initialization

        private static void Initialize()
        {
            var container = ConfigureIoc();
            ConfigureUrchin(container);
            ResolveIocDependencies(container);
        }

        private static void ResolveIocDependencies(UnityContainer container)
        {
            _errorReporter = container.Resolve<IErrorReporter>();
            _dataAccessLayer = container.Resolve<IDataAccessLayer>();
        }

        private static void ConfigureUrchin(UnityContainer container)
        {
            var urchinConfigFile = container.Resolve<FileSource>();
            urchinConfigFile.Initialize(new FileInfo("urchin.json"), TimeSpan.FromSeconds(10));
        }

        private static UnityContainer ConfigureIoc()
        {
            var container = new UnityContainer();
            container.RegisterInstance(container);

            var packageLocator = new PackageLocator().ProbeBinFolderAssemblies().Add(Assembly.GetExecutingAssembly());
            Ioc.Modules.Unity.Registrar.Register(packageLocator, container);

            return container;
        }

        #endregion
    }
}
