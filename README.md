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

## How does Prius stack up
Prius is all about convenience, programmer productivity and runtime performence. I tried
to make it as easy as possible to use and very easy to understand without any compromise
to runtime performance.

In the Visual Studio solution for Prius there are projects that you can run to test the
runtime performance of Prius on your hardware, and compare Prius to other alternatives.
The results of running these tests on my hardware are summarized in the following table:

|Test                          |Iterations |Baseline|Prius   |EF      |ADO.Net |
|------------------------------|-----------|--------|--------|--------|--------|
|Do nothing                    |  1        |   84us |   45us |        |        |
|Do nothing                    |  1000     |    7ns |    5ns |        |        |
|Retrieve one customer         |  1        |  2.7ms |  102ms |        |        |
|Retrieve one customer         |  100      |  0.9us | 0.27ms |        |        |
|One customer with orders      |  1        |    1ms |   13ms |        |        |
|One customer with orders      |  100      |    2us | 0.67ms |        |        |
|Selected customers            |  1        |   18ms |   13ms |        |        |
|Selected customers            |  100      |  1.7ms |   10ms |        |        |
|Selected customers with orders|  1        |  4.6ms |  303ms |        |        |
|Selected customers with orders|  100      |  3.7ms |  284ms |        |        |
|All customers                 |  1        |    1ms |   11ms |        |        |
|All customers                 |  100      |  0.8ms |   10ms |        |        |
|All customers with orders     |  1        |    3ms |  281ms |        |        |
|All customers with orders     |  100      |  1.7ms |  287ms |        |        |

###Notes
Each test was run once and the time taken recorded in this table, then the test was 
run again multiple times and the average time recorded in this table. The tests were
done like that because there is often a startup cost (for example Prius uses reflection
to build a map one time only, so the very first usage takes longer). Most real-world
applications will call the database many times for the same type of data, so the values
in this table for multiple iterations are the most useful ones.

The 'Do nothing' test just tests an empty statement and is included so you can see the
overhead of the testing framework itself. The 'Baseline' project implements the data
access layer by constructing objects and filling them with random data. This enables
you to get an idea of how long that part of the operation takes compared to retrieving 
from the DB and constructing/filling the objects.

I ran these tests on a 4GB Microsoft Surface Pro 3 with i5 processor. 
The software was Microsoft Visual Studio 2013 and SQL Server 2014 Express.

###The performance testing projects contain

Prius.Performance.Shared contains the actual tests. This makes sure there is a level
playing field between the technologies being tested. The test defines a customer with
orders and a data access layer that can retrieve customers and lazily load their orders.
The data access layer is implemented in each technology and the exact same tests are
run against each implementation.

Prius.Performance.Dummy contains a data access layer that does no data access. This can
be used as a baseline for comparing the other technologies.

Prius.Performance.Prius contains a data access layer implementation that uses Prius so
that we can measure how fast Prius is. This project is also a good example of a fairly
minimal application that has Prius integrated into it. It also demonstrates a number
of ways to work with Prius, for example stored procedures that return multiple result
sets, executing multiple queries in parallel, data contracts with injected dependencies etc.

Prius.Performance.EntityFramework contains a data access layer implementation that uses 
the Microsofts Entity Framework so that we can measure how fast it is for the same set of 
tests.

Prius.Performance.Ado contains a data access layer implementation that uses 
the Microsofts ADO.Net Framework that we can measure how fast it is for the same set of 
tests.

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
| `IAsyncEnumerableFactory` | `AsyncEnumerableFactory` |

In addition you must write classes in your application that implement these interfaces,
and register them with your IoC container:

| Interface        | Description |
| ---------------- | ----------- |
| `IFactory`       | Used to construct instances when you map database results onto classes |
| `IErrorReporter` | Used to report errors |

It is recommended that you implement `IFactory` usign your IoC container so that you can
map the results from the database onto objects that have dependencies. If you are mapping
only data contracts with default public constructors, then you can write a simpler and faster
version of `IFactory` that calls the default public constructor instead.

> Note that Prius uses Urchin for its configuration, so Urchin must also be registered in your IoC container. See Urchin documentation for how to do this.

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
                {
				    name:"db1", 
					type:"SqlServer", 
					connectionString:"",
					procedures:
					[
					    {name:"Sproc1", timeout:3},
					    {name:"Sproc2", timeout:7}
					]
				},
                {
				    name:"db2", 
					type:"MySQL", 
					connectionString:"",
					procedures:
					[
					    {name:"Sproc1", timeout:6},
					    {name:"Sproc2", timeout:15}
					]
				},
                {
				    name:"db3", 
					type:"MySQL", 
					connectionString:"",
					procedures:
					[
					    {name:"Sproc1", timeout:6},
					    {name:"Sproc2", timeout:15}
					]
				}
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
                         {sequence:2, databases:["db2","db3"], fallbackPolicy:"backup"}
                     ]
                }
            ]
        }
    }

What this configuration example does is:

1. Defines database connections, one to SqlServer database and two MySQL databases. I left the connection strings blank to keep the example simple.
2. Defines a 'primary' fallback policy that will fall over to the backup server for 1 minute if more than 20% of database requests error or timeout.
3. Defines a 'backup' fallback policy that will not fail over even when the error rate is 100%.
4. Defines a 'users' repository that uses Microsoft SQL server initially, but fails over to a pair of MySQL databases if SQL Server is slow or unavailable.

> Note that because the code you write in your application is identical for all databases, it is possible for Prius to fall back from SQL Server to MySQL.

> Note that for this to work, SQL Server and MySQL must contain all the same stored procedures.

> Note that when you call the `Create()` method of `IContextFactory`, it is the name of the repository that you pass. In this example `_contextFactory.Create("users");`

> Note that you can define timeout values for each stored procedure on each server. Any stored procedures that you dont define timeouts for will default to 5 seconds.
