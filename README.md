# Prius
An Object Relational Mapper (ORM) for people who like stored procedures.

## What Does Prius Do?
Prius is an extremely efficient way of executing queries or stored 
procedures in your database, and mapping the results onto objects. Prius 
provides a lot of convenience. The advanced code generation techniques 
is uses also gives it much higher performance than coding ADO.Net 
the long handed way.

> In benchmarks, Prius was typically between 2 and 4 times faster than using a `SqlConnection` and `DataReader`. This is because Prius uses the Linq compiler to generate machine code at run-time that performs mapping as efficiently as possible.

Apart from being convenient and fast, Prius also adds enterprise level features 
like measuring database performance, and switching to alternate connections, 
and throttling database access to allow the database to recover from 
performance bottlenecks.

## Sample Code
This is an example of calling a stored procedure with some parameters and 
returning the results as a collection of objects:

By default database columns are mapped to properties with the same name:

    using Prius.Contracts.Interfaces;

	internal class Profile
    {
        long ProfileId { get; set; }
        string UserName { get; set; }
    }
    
    internal class MyClass
    {
        private readonly IContextFactory _contextFactory;
        private readonly ICommandFactory _commandFactory;
    
        public MyClass(IContextFactory contextFactory, ICommandFactory commandFactory)
        {
            _contextFactory = contextFactory;
            _commandFactory = commandFactory;
        }
    
        public Profile GetProfile(long profileId)
        {
            try
            {
                using (var context = _contextFactory.Create("MyDatabase"))
                {
                    using (var command = _commandFactory.CreateStoredProcedure("getProfileById"))
                    {
                        command.AddParameter("id", profileId);
                        using(var results = context.ExecuteEnumerable<Profile>(command))
                            return results.FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve profile id " +
                    profileId + " from the database", ex);
            }
        }
    }

You can also use different names in the database and in your C#.

    /// <summary>
    /// An example of a data contract with declarative mapping to sql fields
    /// </summary>
    internal class User
    {
      // Maps the Name property onto the 'userName' column in the database, if
      // the database contains null the Name property will ne Null
      [Mapping("userName")]
      public string Name { get; set; }
    
      // Maps the Age property onto the 'userAge' column in the database. 
      // If the database contains NULL then the Value property will be set to -1
      [Mapping("userAge", -1)]
      public int Age { get; set; }
    
      // Maps the Description property onto the 'descr' column in the
      // database. If the database contains NULL then the Description property 
      // will be set to an empty string
      [Mapping("descr", "")]
      public string Description { get; set; }
    }

For ultimate flexibility you can also implement the `IDataContract` interface

    internal enum Enum1 { Value1, Value2, Value3 }
    
    internal class SampleDataContract: IDataContract<SampleDataContract>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Enum1 MyEnum { get; set; }
    
        public void AddMappings(ITypeDefinition<Contract2> typeDefinition, string dataSetName)
        {
            typeDefinition.AddField("name", c => c.Name, string.Empty);
            typeDefinition.AddField("value", c => c.Value, -1);
            typeDefinition.AddField("descr", (c, v) => c.Description = v.ToLower(), string.Empty);
            typeDefinition.AddField("enum", c => c.MyEnum, Enum1.Value1);
        }
    
        public void SetCalculated(IDataReader dataReader, string dataSetName)
        {
            Title = Name + "=" + Value;
        }
    }

## Building Prius into your application
The recommended method if integration is to use an IoC container - but you do not have to.
If you are using IoC, then you need to map these Prius interfaces onto classes that are
provided by Prius. As always with IoC you can also substitute the Prius implementation
for your own implementation to customize the behaviour.

| Interface                | Class                   |
| ------------------------ | ----------------------- |
| `ICommandFactory`        | `CommandFactory`        |
| `IConnectionFactory`     | `ConnectionFactory`     |
| `IContextFactory`        | `ContextFactory`        |
| `IDataEnumeratorFactory` | `DataEnumeratorFactory` |
| `IDataReaderFactory`     | `DataReaderFactory`     |
| `IMapper`                | `Mapper`                |
| `IParameterFactory`      | `ParameterFactory`      |
| `IRepositoryFactory`     | `RepositoryFactory`     |
| `IEnumerableDataFactory` | `EnumerableDataFactory` |

In addition you must write classes in your application that implement these interfaces,
and register them with your IoC container:

| Interface        | Description |
| ---------------- | ----------- |
| `IFactory`       | Used to construct instances when you map database results onto classes |
| `IErrorReporter` | Used to report errors |

It is recommended that you implement `IFactory` usign your IoC container so that you can
map the results from the database onto objects that have dependencies. If you are mapping
only data contracts with default public constructors, then you can write a simpler and faster
versiono of `IFactory` that calls the default public constructor instead.

## Supported Databases
In this version, Prius supports Microsoft SQL Server, MySQL and Postgresql. This is an open 
source project, and adding support for another database only means adding 3 new source files, 
so please give back to the community if your database is not one of these.

One of the goals of Prius is to make your code identical no matter which database you are 
using. The type of database is simply a configuration option. This makes it especially 
useful for reusable [NuGet](https://www.nuget.org/) packages, where users will prefer to 
stick with their existing database technology.

## Configuration
Prius uses [Urchin](https://www.nuget.org/packages/Urchin.Client/ "Urchin") for it's 
configuration. Urchin allows all application configuration to be stored in a central 
server, and also allows configuration for different applications, machines and environments
to be specified using rules to avoid duplication of configuration.

Another feature of Urchin that Prius makes use of is the configuration change notification.
Whenever the configuration of an application that uses Prius is changed, Prius will start
using the new configuration within a few seconds without restarting the application. This
will allow you for example to fail all applications over to a backup database server by
changing a rule on the Urchin configuration service.

This is a sample Urchin configuration for Prius:

    {
        prius:{
            databases:[
                {name:"db1", type:"SqlServer", connectionString:""},
                {name:"db2", type:"MySQL", connectionString:""}
            ],
            fallbackPolicies:[
                {name:"primary", allowedFailurePercent:20, backOffTime:"00:01:00"},
                {name:"backup", allowedFailurePercent:100}
            ],
            repositories:[
                {
                     name:"users",
                     clusters:[
                         {sequence:1, databases:["db1"], fallbackPolicy:"primary"},
                         {sequence:2, databases:["db2"], fallbackPolicy:"backup"}
                     ]
                }
            ]
        }
    }

What this configuration example does is:

1. Defines two database connections, one to SqlServer and one to MySQL. I left the connection strings blank to keep the example simple.
2. Defines a 'primary' fallback policy that will fall over to the backup server for 1 minute if more than 20% of database requests error or timeout.
3. Defines a 'backup' fallback policy that will not fail over even when the error rate is 100%.
4. Defines a 'users' repository that uses Microsoft SQL server, but fails over to MySQL if SQL Server is slow or unavailable.

> Note that because the code you write in your application is identical for all databases, it is possible for Prius to fall back from SQL Server to MySQL.

> Note that for this to work, SQL Server and MySQL must contain all the same stored procedures.

> Note that when you call the `Create()` method of `IContextFactory`, it is the name of the repository that you pass. In this example `_contextFactory.Create("users");`
