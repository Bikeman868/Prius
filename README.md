# Prius
An Object Relational Mapper (ORM) for people who like stored procedures.

## What Does Prius Do?
Prius is an extremely efficient way of executing queries or stored 
procedures in your database, and mapping the results onto objects. Prius 
provides a lot of convenience. The advanced code generation techniques 
is uses also gives it much higher performance than coding ADO.Net 
the long handed way.

Apart from being convenient and fast, Prius also adds enterprise level features 
like measuring database performance, and switching to alternate connections, 
and throttling database access to allow the database to recover from 
performance bottlenecks.

## Prius 2

Note that in Prius 1 all of the database drivers were included in the main
NuGet package. This had the advantage that it's one package to install which 
is simple, but has the disadvantage that your application has dependencies
on MySql and PostgreSQL even if you don't use these databases.

In Prius 2 I am planning to expand to more database servers, starting with
SqlLite. The model of including all database drivers in the one assembly works
less well as I add more database drivers, to I split these out into separate
NuGet packages.

If you are updating from Prius 1 to Prius 2, you will need to add the NuGet
packages for the databases that you use or you will get a runtime error when
the first database connection is attempted.

If the Prius configuration file there is `type` property for each `database`. 
The table below lists the values of this `type` property and the corresponding
NuGet package that you need to install.

| Database `type` | NuGet package name |
|-----------------|--------------------|
| SqlServer       | Prius.SqlServer    |
| MySql           | Prius.MySql        |
| PostgreSql      | Prius.PostgreSQL   |

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
    /// An example of a data contract with declarative mapping to DB columns
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

> Note that column names are case insensitive.

> Note that if you don't add any `[Mapping]` attributes to your data contract
> then Prius will map all public properties into database columns with the same
> names. If you add one or more `[Mapping]` attributes then properties without
> `[Mapping]` attributes will not be mapped and will not have their values
> set by Prius (unless you also implement `IDataContract` - see below).

> Note that you can add multiple `[Mapping]` attributes to one property, in
> which case database columns with any of these names will map to this property.
> This is useful when different stored procedures return the same column with
> different names because of column aliasing.

Note that Prius also allows you to add declarative field mappings to interfaces
instead of concrete types. Note that when you do this the `IFactory` that you 
provide to Prius must be capable of constructing objects from interface types.
To use the mappings defined for an interface, pass the interface type when
calling the `ExecuteEnumerable()` method.

You might want to use interface data contracts instead of concrete class types because:

1. You want to have more than one way to map to a class onto the database. You 
can make the class implement multiple interfaces and decorate each interface 
with different `[Mapping]` attributes.

2. You want to return objects whose type is imported from a DLL that you do not
have source code for and therefore can not have `[Mapping]` attributes added
to it. In this case you can inherit from the imported type and
add an interface to the derrived class decorated with `[Mapping]` attributes.
This is more efficient than defining a new class and copying the data using
something like `AutoMapper`.

3. You just prefer using interfaces. There is nothing wrong with concrete data
contracts provided they do not define any methods, but some developers prefer to
define everything in terms of interfaces.

For greater flexibility you can also implement the `IDataContract` interface, in
this case any declarative mappings will be applied first, then your `IDataContract`
implementation will execute, and can modify the mappings.

    internal enum Enum1 { Value1, Value2, Value3 }
    
    internal class SampleDataContract: IDataContract<SampleDataContract>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Enum1 MyEnum { get; set; }
    
        public void AddMappings(ITypeDefinition<SampleDataContract> typeDefinition, string dataSetName)
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

For ultimate flexibility you can also implement the `IDataContract` interface in a way that
creates different column mappings for different stored procedures. This is useful only
when your database returns different data in the same column names depending on
which stored procedure you call, for example if some stored procedures return age as a string
and others return age as an integer even though they both return essentially the same data
(this would be pretty messed up I know, but I have had to work with legacy databases that have 
these kinds of problems).

    internal enum Enum1 { Value1, Value2, Value3 }
    
    internal class SampleDataContract: IDataContract<SampleDataContract>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Enum1 MyEnum { get; set; }
    
        public void AddMappings(ITypeDefinition<SampleDataContract> typeDefinition, string dataSetName)
        {
			if (string.Equals(dataSetName, "someWeirdSproc", StringComparison.OrdinalIgnoreCase))
			{
				typeDefinition.AddField("n", c => c.Name, string.Empty);
				typeDefinition.AddField("v", c => c.Value, -1);
				typeDefinition.AddField("d", (c, v) => c.Description = v.ToLower(), string.Empty);
				typeDefinition.AddField("e", c => c.MyEnum, Enum1.Value1);
			}
			else
			{
				typeDefinition.AddField("name", c => c.Name, string.Empty);
				typeDefinition.AddField("value", c => c.Value, -1);
				typeDefinition.AddField("descr", (c, v) => c.Description = v.ToLower(), string.Empty);
				typeDefinition.AddField("enum", c => c.MyEnum, Enum1.Value1);
			}
        }
    
        public void SetCalculated(IDataReader dataReader, string dataSetName)
        {
            Title = Name + "=" + Value;
        }
    }


## How does Prius compare with the alternatives
Prius is all about convenience, programmer productivity and runtime performence. I tried
to make it as easy as possible to use and very easy to understand without any compromise
to runtime performance.

## Feature comparison matrix

| Feature                                         | Prius | ADO.Net | nHibernate | EF  |
|-------------------------------------------------|-------|---------|------------|-----|
| Supports SQL Server                             | Yes   | Yes     | Yes        | Yes |
| Supports MySQL                                  | Yes   | Yes     | Yes        | Yes |
| Supports Postgsql                               | Yes   | Yes     | Yes        | Yes |
| Easy to add support for other databases         | Yes   | No      | No         | No  |
| Provides load balancing                         | Yes   | No      | No         | No  |
| Provides fail over                              | Yes   | No      | No         | No  |
| Monitors database health                        | Yes   | No      | No         | No  |
| Can map results from queries onto objects       | Yes   | No      | Yes        | Yes |
| Has built-in lazy loading                       | No    | No      | Yes        | Yes |
| Can execute parameterized ad-hoc queries        | Yes   | Yes     | Yes        | Yes |
| Can execute stored procedures                   | Yes   | Yes     | Yes        | Yes |
| Can execute asynchronous requests               | Yes   | Yes     | Yes        | Yes |
| Can generate efficient SQL from LINQ            | No    | No      | Yes        | Yes |
| Can use IoC container to construst data models  | Yes   | No      | No         | No  |
| Can generate database from data model           | No    | No      | Yes        | Yes |
| Can generate data model from the database       | No    | No      | Yes        | Yes |
| Can fill any class with query results           | Yes   | No      | No         | No  |
| Can use data model from external library        | Yes   | No      | No         | No  |
| Can fill objects using results from multiple stored procedure calls | Yes   | No      | No         | No  |
| Can easily handle stored procedures that return multiple data sets | Yes   | No      | No         | No  |
| Separates code and configuration for things like stored procedure timeout | Yes   | No      | No         | No  |

EF refers to the Microsoft Entity Framework.

## Performance comparison

In the Visual Studio solution for Prius there are projects that you can run to test the
runtime performance of Prius on your hardware, and compare Prius to other alternatives.
The results of running these tests on my hardware are summarized in the following table:

|Test                                |Iterations |Baseline|Prius   |EF      |ADO.Net |
|------------------------------------|-----------|--------|--------|--------|--------|
|Do nothing                          |  1        |   51us |   32us |        |   39us |
|Do nothing                          |  1000     |    6ns |    5ns |        |    6ns |
|Retrieve one customer               |  1        |  1.8ms |   93ms |        |   78ms |
|Retrieve one customer               |  100      |  0.9us | 0.22ms |        | 0.25ms |
|One customer with orders            |  1        |  2.2ms |   14ms |        |  7.2ms |
|One customer with orders            |  100      |  4.8us | 0.76ms |        | 0.22ms |
|Selected customers                  |  1        |   12ms |   19ms |        | 11.5ms |
|Selected customers                  |  100      |  1.7ms |  6.8ms |        |  3.3ms |
|Selected customers lazy load orders |  1        |  2.9ms |  8.1ms |        |  8.6ms |
|Selected customers lazy load orders |  100      |  1.7ms |  6.7ms |        | 10.4ms |
|Selected customers with orders      |  1        |  3.4ms |  134ms |        |  794ms |
|Selected customers with orders      |  100      |  2.5ms |  104ms |        |  589ms |
|All customers                       |  1        |  1.1ms |  6.1ms |        |  3.2ms |
|All customers                       |  100      |  1.2ms |  9.6ms |        |  2.5ms |
|All customers with orders           |  1        |    3ms |   99ms |        |  501ms |
|All customers with orders           |  100      |  1.2ms |  101ms |        |  513ms |

### Performance Testing Notes
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

Prius uses ADO.Net to connect to the database, and adds an object mapping layer on top.
You can see the cost of the ORM and other Prius features (such as load balancing
and failover) by comparing Prius with ADO.Net.

I ran these tests on a 4GB Microsoft Surface Pro 3 with i5 processor and an external
240GB SSD connected via USB 3.0. The software was Windows 10, Visual Studio 2013 and 
SQL Server 2014 Express.

### The performance testing projects

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

### Using `Ioc.Modules`

If you are already using the `Ioc.Modules` NuGet package in your application then most
of the IoC configuration will happen automatically. You will need to implement two interfaces
in your application, and add them to an `Ioc.Modules` package class.

The interfaces you need to implement are:

| Interface        | Description |
| ---------------- | ----------- |
| `IFactory`       | Used to construct instances when you map database results onto classes. You only have to implement a couple of very simple methods. |
| `IErrorReporter` | Used to report errors. This interface also defines a couple of very straightforward methods. |

Your `Package.cs` file should look something like this:

    using System.Collections.Generic;
    using Ioc.Modules;
    using Prius.Contracts.Interfaces.External;
    
    namespace MyApp
    {
        [Package]
        public class Package: IPackage
        {
            public string Name { get { return "My application"; } }
    
            public IList<IocRegistration> IocRegistrations
            {
                get 
                {
                    return new List<IocRegistration>
                    {
                        new IocRegistration().Init<IFactory, PriusFactory>(),
                        new IocRegistration().Init<IErrorReporter, PriusErrorReporter>(),
                    };
                }
            }
        }
    }

### Without using `Ioc.Modules`

The recommended method if integration is to use an IoC container - but you do not have to.
If you are using IoC, then you need to map these Prius interfaces onto classes that are
provided by Prius. As always with IoC you can also substitute the Prius implementation
for your own implementation to customize the behaviour.

| Interface                 | Class                    |
| ------------------------- | ------------------------ |
| `ICommandFactory`         | `CommandFactory`         |
| `IConnectionFactory`      | `ConnectionFactory`      |
| `IContextFactory`         | `ContextFactory`         |
| `IDataEnumeratorFactory`  | `DataEnumeratorFactory`  |
| `IDataReaderFactory`      | `DataReaderFactory`      |
| `IMapper`                 | `Mapper`                 |
| `IParameterFactory`       | `ParameterFactory`       |
| `IRepositoryFactory`      | `RepositoryFactory`      |
| `IEnumerableDataFactory`  | `EnumerableDataFactory`  |
| `IAsyncEnumerableFactory` | `AsyncEnumerableFactory` |

In addition you must write classes in your application that implement the interfaces described 
above in the section about using `Ioc.Modules`

It is recommended that you implement `IFactory` using your IoC container so that you can
map the results from the database onto objects that have dependencies. If you are mapping
only data contracts with default public constructors, then you can write a simpler and faster
version of `IFactory` that calls the default public constructor instead.

> Note that Prius uses Urchin for its configuration, so Urchin must also be registered in your IoC container. 
> See Urchin documentation for how to do this. Note that if you are using `Ioc.Modules` then Urchin will
> be configured in your IoC container automatically.

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
```
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
```
What this configuration example does is:

1. Defines database connections, one to SqlServer database and two MySQL databases.
I left the connection strings blank to keep the example simple.

2. Defines a 'primary' fallback policy that will fall over to the backup server 
for 1 minute if more than 20% of database requests error or timeout.

3. Defines a 'backup' fallback policy that will not fail over even when the error rate is 100%.

4. Defines a 'users' repository that uses Microsoft SQL server initially, but fails 
over to a pair of MySQL databases if SQL Server is slow or unavailable.

> Note that because the code you write in your application is identical for all 
> databases, it is possible for Prius to fall back from SQL Server to MySQL.

> Note that for this to work, SQL Server and MySQL must contain all the same stored procedures.

> Note that when you call the `Create()` method of `IContextFactory`, it is the 
> name of the repository that you pass. In this example `_contextFactory.Create("users");`

> Note that you can define timeout values for each stored procedure on each server. 
> Any stored procedures that you dont define timeouts for will default to 5 seconds.

> Note that you can also pass a timeout value in the code that calls the 
> stored procedure, but this is generally less maintainable than the 
> configuration based approach. Remember that Prius uses the Urchin rules 
> based configuration management system that can define environment specific rules.

##Prius recipies

###Inject Prius interfaces into my data access class
```
    using Prius.Contracts.Interfaces;

    public class DataAccessLayer : IDataAccessLayer
    {
        private readonly IContextFactory _contextFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly IMapper _mapper;
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;
    
        public DataAccessLayer(
            IContextFactory contextFactory,
            ICommandFactory commandFactory,
            IMapper mapper,
            IDataEnumeratorFactory dataEnumeratorFactory)
        {
            _contextFactory = contextFactory;
            _commandFactory = commandFactory;
            _mapper = mapper;
            _dataEnumeratorFactory = dataEnumeratorFactory;
        }
    }
```
> Note that you always need `IContextFactory` and `ICommandFactory`. The other interfaces are only needed for some more advanced techniques.

###Execute a stored procedure and return a list of objects
```
    public IList<ICustomer> GetCustomers()
    {
        using (var context = _contextFactory.Create("MyData"))
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_GetAllCustomers"))
            {
                using (var data = context.ExecuteEnumerable<Customer>(command))
                    return data.ToList();
            }
        }
    }
```

###Execute a stored procedure and return a single object
```
    public ICustomer GetCustomer(int customerId)
    {
        using (var context = _contextFactory.Create("MyData"))
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_GetCustomer"))
            {
    			command.AddParameter("CustomerID", customerId);
                using (var data = context.ExecuteEnumerable<Customer>(command))
                    return data.FirstOrDefault();
            }
        }
    }
```

##Execute a stored procedure that returns no data
```
    public ICustomer DeleteCustomer(int customerId)
    {
        using (var context = _contextFactory.Create("MyData"))
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_DeleteCustomer"))
            {
    			command.AddParameter("CustomerID", customerId);
                context.ExecuteNonQuery(command));
            }
        }
    }
```

###Execute a stored procedure with output parameters
```
    public bool InsertCustomer(ICustomer customer)
    {
        using (var context = _contextFactory.Create("MyData"))
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_InsertCustomer"))
            {
    			var idParam = command.AddParameter("CustomerID", SqlDbType.Int);
                var rowsAffected = context.ExecuteNonQuery(command));
    
    			if (rowsAffected != 1)
    			    return false;
    
    			customer.CustomerId = (int)idParam.Value;
    			return true;
            }
        }
    }
```

###Execute a stored procedure that returns multiple sets of data
Note that the `context.ExecuteEnumerable` method is a shorthand syntax that works for the most common use case of a 
single data set. To work with multiple sets of data you have to use a slightly more verbose syntax, but this results
in the same internal operation.

In this example the stored procedure returns a single customer record in the first data set and a list of the 
customer's orders in the second data set. This example therefore demonstrates two different techniques
```
    public ICustomer GetCustomerAndOrders(int customerId)
	{
       using (var command = _commandFactory.CreateStoredProcedure("dbo.sp_GetCustomerAndOrders"))
       {
           command.AddParameter("CustomerID", customerId);
           using (var context = _contextFactory.Create("MyData"))
           {
               using (var reader = context.ExecuteReader(command))
               {
                   if (reader.Read())
                   {
                       var customer = _mapper.Map<Customer>(reader);
                       if (reader.NextResult())
                       {
                           using (var orderEnumerator = _dataEnumeratorFactory.Create<Order>(reader))
                               customer.Orders = orderEnumerator.Cast<IOrder>().ToList();
                       }
                       return customer;
                   }
               }
           }
       }
    }

```

###Execute two stored procedures at the same time
Note that the database context can only have one open data reader at a time, so you will need multiple context 
objects to execute multiple stored procedures concurrently.
You could nest using statements, but this can get very deep if you have many concurrent requests. In this example
I used a try...finally instead.

Note that if you are using .Net 4.5 or higher then you can use the async...await mechanism for this instead.

```
    public ICustomer GetCustomersAndOrders(int customerId)
    {
        var customerContext = _contextFactory.Create("MyData");
        var customerCommand = _commandFactory.CreateStoredProcedure("dbo.sp_GetCustomer");
		customersCommand.AddParameter("CustomerID", customerId);

        var ordersContext = _contextFactory.Create("MyData");
        var ordersCommand = _commandFactory.CreateStoredProcedure("dbo.sp_GetCustomerOrders");
		ordersCommand.AddParameter("CustomerID", customerId);
    
        try
        {
            var customerResult = customerContext.BeginExecuteEnumerable(customerCommand);
            var ordersResult = ordersContext.BeginExecuteEnumerable(ordersCommand);
            WaitHandle.WaitAll(new[] { customerResult.AsyncWaitHandle, ordersResult.AsyncWaitHandle });
    
			Customer customer;
            using (var customerRecords = customerContext.EndExecuteEnumerable<Customer>(customerResult))
                customer = customerRecords.FirstOrDefault();

			if (customer == null)
				return null;

            using (var orderRecords = ordersContext.EndExecuteEnumerable<Order>(ordersResult))
                customer.Orders = orderRecords.ToList();
        }
        finally
        {
            customerContext.Dispose();
            customerCommand.Dispose();
            ordersContext.Dispose();
            ordersCommand.Dispose();
        }
    }
```

###Combine the results of two or more stored procedures into one object
This example assumes that news articles have a very large 'content' column that isn't required most of the time,
so the `sp_GetNewsArticle` stored procedure does not return the 'content' column. There is a separate stored 
procedure that only returns the 'content' column.

Note that in this example both stored procedures are in the same database, but you
could have the second stored procedure in a different database, and this database could even use a different
database technology.

```
    public class NewsArticle
    {
        public long NewsArticleId { get; set; }
        public DateTime PublishedDate { get; set; }
        public string Source { get; set; }
        public string Headline { get; set; }
        public string Content { get; set; }
    }
    
    public NewsArticle GetNewsArticle(long newsArticleId, bool includeContent = false)
    {
        using (var context = _contextFactory.Create("MyData"))
        {
            NewsArticle newsArticle;
            using (var command = _commandFactory.CreateStoredProcedure("sp_GetNewsArticle"))
            {
                command.AddParameter("NewsArticleID", newsArticleId);
                using (var data = context.ExecuteEnumerable<NewsArticle>(command))
                    newsArticle = data.FirstOrDefault();
            }
    
            if (includeContent && newsArticle != null)
            {
                using (var command = _commandFactory.CreateStoredProcedure("sp_GetNewsArticleContent"))
                {
                    command.AddParameter("NewsArticleID", newsArticleId);
                    using (var reader = context.ExecuteReader(command))
                    {
                        if (reader.Read())
                            _mapper.Fill(newsArticle, reader);
                    }
                }
            }
    
            return newsArticle;
        }
    }
```

###Execute a stored procedure and map DB columns using an interface
```
	public interface ICustomer
	{
		[Mapping("fld_CustomerID")]
		long Id { get; set; }

		[Mapping("fld_CustomerName")]
		string Name { get; set; }
	}

	internal class Customer: ICustomer
	{
		public long Id { get; set; }
		pulic string Name { get; set; }
	}

    public IList<ICustomer> GetCustomers()
    {
        using (var context = _contextFactory.Create("MyData"))
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_GetAllCustomers"))
            {
                using (var data = context.ExecuteEnumerable<ICustomer>(command))
                    return data.ToList();
            }
        }
    }
```
