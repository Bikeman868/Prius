# SqLite Driver for Prius

Prius is an ORM for people that like stored procedures, but SqLite has 
no support for stored procedures, so what it this library all about?

The main advantage of stored procedures are that they are stored in
the database (and can be modified independant of the application)
and they are executed on the database server (which means that the
data they access does not have to be transmitted over the wire). Since
SqLite is a DLL that executes within the application, and the data
is stored locally anyway, there seems to be no advantage to using
stored procedures with SqLite, however...

I started this project because I was in a position where:

* We had a set of services that acted like in-memory databases with 
built-in custom application logic. These services were backed by
MariaDB for persistence.

* The MariaDB was only accessed by the one specific in-memory database 
service, so client/server was not required, and it could therefore be 
replaced by SqLite without any architectural changes.

* The services use Prius to access MariaDB and only have access to 
stored procedures with no direct SQL execution. The stored procedures
contain a lot of business logic not only CRUD operations.

* MariaDB was the weakest link in our system performance and reliability
by far. After many efforts to make it work reliably at high load we
decided that we had to switch to something else. After some research I
decided that SqLite might be the answer and wanted to try it, but I also
wanted to leave open the option to switch to something else later if 
it wasn't the right solution.

I decided the best way forwards was to build a driver for Prius that
would use SqLite for storage and allow 'stored procedures' to be written
in C#. I put 'stored procedures' in quotes because they are not stored,
they are written in C# and compiled into the application, but they
perform the same function as stored procedures. Building a Prius driver
for SqLite and re-creating my stored procedures in C# meant that there 
were no changes at all to the application and I can switch between SqLite, 
MariaDB, MySQL and SQL Server just through configuration changes.

This solution was a good fir for my rather unusual situation. These are 
some other scenarios in which I think this solution might also be a good fit:

* If you want to use SqLite for performance and stability but also
keep an option open to move up to a client/server model later.

* You have an application that needs to run in both enterprise and stand
alone configurations. In this case you can configure it to connect to a 
database like Oracle or MySQL and put business logic into stored procedures 
for enterprise configurations. For the stand-alone deployment scenario
configure Pruis to use SqLite and ship a DLL containing the 'stored procedures'
written in C#. In this case the same compiled binaries will work for both
scenarios.

* I am planning to add the ability to store procedures in the SqLite
database, and to have these compiled and cached at runtime. This will
bring back the usual advantages of stored procedures.

* During devlopment and testing you might find it more convenient to
use SqLite (just delete the database file and it will be re-created
next time you run the application). You can still deploy in production
using a client/server database like SQL Server.

* You like the idea of a clean separation between database logic and
application logic, and prefer to write, test and debug stored procedures
written in C# and compiled into the application. Note that your 
database logic can make use of domain model objects in this scenario.

## What does it do?

This is a plug-in driver for the Prius ORM that allows you to configure
SqLite databases in your application. SqLite is probably the most widely
used database engine in the world, and is considered to be one of the
most stable and reliable. SqLite is written in C and compiled into a
DLL. There are a couple of .Net wrappers around the SqLite DLL. This
package uses `System.Data.SQLite` which emulates ADO.Net, this made it
easy to integrate with Prius.

## Future plans

Eventually I want to allow procedures to be written in a T-SQL
like language and be stored in the SqLite database. I also want
to build a client/server database solution that uses SqLite as the 
engine on the server side, and uses this library for stored procedure
support. I think this would be much more robust than some of the
other client/server database choices available.

At some point I will investigate using the sqlite3.dll directly instead
of going through `System.Data.SQLite`. If there are significant performance
or scalability gains then I would switch to this approach.

## Getting started

How to get started depends a bit on where you already are and where you
are trying to get to. I can't cover all the possibilities here so please
contact me for advice about your particular situation.

### Getting started from scratch

If you are starting a new application and you want to use SqLite for
relational storage, Prius will provide the the following benefits:

1. You can define your database schema by creating classes and decorating
them with attributes. Prius will create the SqLite database with all of the
tables and indexes just by examining your data contract classes.

2. If you modify your database schema, Prius will detect this at startup
and add, remove or update database tables and columns automatically.

3. You can separate application logic from database logic (both written
in C# or VB)  and later if you want to move to a client/server solution
like MySQL or SQL Server you can translate the database logic into
stored procedures and you won't have to make any changes to the code
for the application logic.

4. You can build and ship different versions of the database logic because
there is no coupling between the application logic and database logic
exactly like in a client/server database management system.

### Converting from a client/server database

If you already use Prius ORM to connect to a client/server database
such as MySQL or SQL Server, then you can switch to using SqLite
for storage with no modifications to your existing code.

The changes you will need to make can include:

* If you use stored procedures then you will need to re-write them
in C# and include them with your deployment. They can be compiled
into a separate DLL that is not referenced by your solution and dropped
into the `bin` folder. You can also have different versions of this
dll and you can replace it without deploying a new version of your
application.

* If your application executes SQL statements directly, then you need
to check that the syntax of these SQL statements works with SqLite. I 
would encourage you to change this code into a stored procedure call
and encapsulate the SQL statements in a SqLite specific DLL instead 
because all database engines support slightly different syntax.

* If you want this package to create the SqLite database schema for
you, then you need to decorate your data contracts with attributes that
provide information about primary keys, indexes and unique constraints etc.
If you plan to maintain the SqLite database schema in a different way
then you can skip this step.

### Integrating this package into your application

If you are already using Prius ORM them all you need to do is add
this NuGet package to your solution and it will be detected and
registered at runtime.

To configure a SqLite reopsitory in Prius use the server type of 
`SqLite`, for example:

    "prius": 
    {
        "databases": 
        [
            {
                "name": "Users",
                "type": "SqLite",
                "connectionString": "Data Source=Users.sqlite;Version=3;New=True;"
            }
        ],
        "fallbackPolicies": 
        [
            { "name": "noFallback", "allowedFailurePercent": 100 }
        ],
        "repositories": 
        [
            {
                "name": "Users",
                "clusters": [{ "sequence": 1, "databases": [ "Users" ], "fallbackPolicy": "noFallback" }]
            }
        ]
    }

Because the SqLite engine is unmanaged code, you need different binaries
for 32-bit and 64-bit versions of Windows. Unfortunately NuGet does not
provide a way to do this (see https://github.com/aspnet/dnx/issues/402)
so for now you will have to manually copy `SQLite.Interop.dll` into your
`bin` folder. You can also make `x86` and `x64` sub-folders inside your 
`bin` folder, and the SQLite managed wrapper will detect the 32/64 bitedness 
of the OS and load the appropriate version.

When you install the `Prius.SQLite` NuGet package it will create a `lib` folder
in your project directory containing these files. To copy these to the `bin`
folder during build, open the project properties, navigate to post build steps
and add `xcopy $(ProjectDir)lib $(TargetDir) /S /C /I /Y` to your post build steps.

You can also download these files from https://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki

# Creating and maintaining the database schema automatically

This package contains a class that will examine the database on
the first connection and compare its schema to the application's
data contracts. It will then adjust the database schema to match
the application.

To turn this feature off, write your own implementation of 
`ISchemaUpdater` and register your version with IoC. The
`ISchemaUpdater` interface only defines one very simple method.

To take advantage of this feature, decorate your data contracts
with attributes like this:

    using System;
    using System.Data;
    using Prius.SqLite.SchemaUpdating;
    
    namespace MyApp
    {
        [SchemaTable("tb_Users")]
        public class User
        {
            [SchemaColumn("userID", DbType.UInt32, ColumnAttributes.UniqueKey)]
            public int? UserId { get; set; }
    
            [SchemaColumn("firstName", DbType.String, ColumnAttributes.NotNull)]
            [SchemaIndex("ix_FullName", IndexAttributes.Unique)]
            public string FirstName { get; set; }
    
            [SchemaColumn("lastName", DbType.String, ColumnAttributes.NotNull)]
            [SchemaIndex("ix_FullName", IndexAttributes.Unique)]
            public string LastName { get; set; }
    
            [SchemaColumn("dateOfBirth", DbType.DateTime)]
            public DateTime DateOfBirth { get; set; }
        }
    }

# Simulating stored procedures

SqLite does not natively support stored procedures because it runs in the address
space of the application already. This package allows you to simulate stored
procedure execution so that you can write a single application that can be run
against a client/server database that does support stored procedures or you can
run against SqLite without any modifications.

It is possible to deploy your 'stored procedures' in a separate DLL and this is
my recommendation. This DLL does not need to be referenced at all by your application
and if you do it this way, then your application does not need any references to
`System.Data.SQLite`.

Each stored procedure is a class. The class must be decorated with a `Procedure`
attribute and must implement an interface that inherits from `IProcedure`. At this
time there are two interfaces defined that inherit from `IProcedure` but the only
one that is supported is `IAdoProcedure`.

Here is an example stored procedure:

    [Procedure("sp_DeleteUser")]
    [Parameter("UserID", typeof(long))]
    public class DeleteUser : IAdoProcedure
    {
        private readonly IAdoQueryRunner _queryRunner;
        private IQuery _deleteSql;

        public DeleteUser(IAdoQueryRunner queryRunner, IQueryBuilder queryBuilder)
        {
            _queryRunner = queryRunner;

            _deleteSql = queryBuilder
                .DeleteFrom("tb_Users")
                .Where("UserID = @UserID");
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            _queryRunner.ExecuteNonQuery(context, _deleteSql);
            return null;
        }
    }

The `Parameter` attribute in the example above is optional. If they are present then
the parameters passed to the procedure will be validated. These attributes also
provide useful documentation.

The `IAdoProcedure` interface only defines one method with one parameter and a return
type, so it's not very challenging to implement, but there are also a bunch of helper
interfaces that you can inject via IoC to help with writing your stored procedure. These are:

* `IQueryBuilder` is useful if you are not very familiar with SqLite SQL syntax. It does
not cover every aspect of the syntax diagrams (for example building expressions) but
it does help with the overall structure of your query. One of the challenges with SqLite is
that it provides very poor feedback when syntax is incorrect and this can be frustrating,
so help in this area is great when you are getting started.

* `IAdoQueryRunner` makes it easier to execute queries against the SqLite engine. Again,
this is totally optional, you can call the APIs directly instead if you want.

* `IDataReaderFactory` helps you to create the `IDataReader` that your procedure should
return.

* `IParameterAccessor` makes it easier to deal with the parameters that are passed to
the procedure, including setting output parameters and supplying a return value.

You can also inject any other interfaces using IoC, so your stored procedures can make use
of any other code in your application.

The example below uses these interfaces:

    [Procedure("sp_InsertUser")]
    [Parameter("FirstName", typeof(string))]
    [Parameter("LastName", typeof(string))]
    [Parameter("DateOfBirth", typeof(DateTime),ParameterDirection.Input, false)]
    [Parameter("UserID", typeof(long), ParameterDirection.ReturnValue)]
    public class InsertUser : IAdoProcedure
    {
        private readonly IAdoQueryRunner _queryRunner;
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IParameterAccessor _parameterAccessor;

        private readonly IQuery _insertSql;
        private readonly IQuery _selectSql;

        public InsertUser(
            IAdoQueryRunner queryRunner, 
            IDataReaderFactory dataReaderFactory, 
            IParameterAccessor parameterAccessor, 
            IQueryBuilder queryBuilder)
        {
            _queryRunner = queryRunner;
            _dataReaderFactory = dataReaderFactory;
            _parameterAccessor = parameterAccessor;

            _insertSql = queryBuilder
                .InsertInto("tb_users", "FirstName", "LastName", "DateOfBirth")
                .Values("@FirstName", "@LastName", "@DateOfBirth");

            _selectSql = queryBuilder
                .Select("UserID", "FirstName", "LastName", "DateOfBirth")
                .From("tb_Users")
                .Where("FirstName = @FirstName")
                .And("LastName = @LastName");
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            _queryRunner.ExecuteNonQuery(context, _insertSql);
            _parameterAccessor.Return(context.Parameters, context.Connection.LastInsertRowId);

            var sqLiteReader = _queryRunner.ExecuteReader(context, _selectSql);
            return _dataReaderFactory.Create(sqLiteReader, context);
        }
    }

# Flexibility options

Prius uses `IoC.Modules` to wire everything up. This means that you can swap
out any interface for your own application specific version and everything inside
this package will use your version.

The interfaces you can usefully provide implementations for are:

`IColumnTypeMapper` is responsible for mapping `System.Data.DbType` to the SqLite data type.

`IQueryRunner` is responsible for submitting queries to the SqLite engine.

`ICommandProcessorFactory` is responsible for constructing an object that will 
handle a request. The default implementation uses `System.Data.SQLite` to 
execute SQL statements and uses `IProcedureLibrary` and `IProcedureRunner` for 
stored procedures.

`ISchemaEnumerator` is responsible for discovering the database schema that is
expected by the application. The default implementation will load and examine
all DLLs in the `bin` folder looking for classes are decorated with
special schema attributes. These attributes define the expected database schema.

`ISchemaUpdater` is responsible for comparing the actual database schema against
the schema that the application was compiled for, and modifying the database
schema to match the application.

`IProcedureLibrary` is responsible for loading, compiling, constructing and pooling
stored procedure instances. The default implementation will scan DLLs in the `bin`
folder and discover all classes that implement `IProcedure` and are decorated with
the `[Procedure()]` attribute.

`IProcedureRunner` is responsible for executing procedures in response to the
application executing stored procedure calls.
