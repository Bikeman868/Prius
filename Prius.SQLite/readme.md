# SqLite Driver for Prius

Prius is an ORM for people that like stored procedures, but SqLite has 
no support for stored procedures, so what it this library all about?

The main advantage of stored procedures are that they are stored in
the database (and can be modified without altering the application)
and they are executed on the database server (which means that the
data they access does not have to be transmitted over the wire). Since
SqLite is a DLL that executes within the application, and the data
is stored locally anyway, there seems to be no advantage to using
stored procedures with SqLite.

I started this project because I was in a position where:

* we had a set of services that acted like in-memory databases with 
built-in custom application logic. These services were backed by
MariaDB for persistence.

* The MariaDB was only accessed by the one specific in-memory database 
service and could therefore be replaced by SqLite without any architectural 
changes.

* The services use Prius to access MariaDB and only have access to 
stored procedures with no direct SQL execution. The stored procedures
contain a lot of business logic, not only CRUD operations.

* MariaDB was the weakest link in our system performance and reliability
by far. After many efforts to make it work reliably at high load we
decided that we had to switch to something else. After some research I
decided that SqLite might be the answer and wanted to try it, but I also
wanted to leave open the option to switch to something else later if 
it wasn't the right solution.

I decided the best way forwards was to build a driver for Prius that
would use SqLite for storage and allow 'stored procedutes' to be written
in C#. By doing this there were no changes to the application and I
can switch between SqLite, MariaDB, MySQL and SQL Server just through
configuration changes.

These are some of the other scenarios in which I think this package
might be a good fit:

* If you want to use SqLite for performance and stability but also
keep an option open to move up to a client/server model later.

* You have an application that needs to run in both enterprise and stand
alone configurations. In this case you can configure it for the enterprise
to connect to a database like Oracle or MySQL and put business logic
into stored procedures. For the stand-alone deployment scenario
configure Pruis to use SqLite and ship a DLL containing the 'stored procedures'
written in C#.

* I am planning to add the ability to store procedures in the SqLite
database, and to have these compiled and cached at runtime. This will
bring back the usual advantages of stored procedures.

* During devlopment and testing you might find it more convenient to
use SqLite (just delete the database file and it will be re-created
next time you run the application). You can still deploy in
production using a client/server database like SQL Server.

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
package uses System.Data.SQLite which emulates ADO.Net and made it
easy to integrate with Prius.

## Future plans

Eventually I want to allow procedures to be written in a T-SQL
like language and be stored in the SqLite database. I also want
to build a client/server database solution that uses SqLite as the 
engine on the server side, and uses this library for stored procedure
support. I think this would be much more robust than some of the
other client/server database choices available.

At some point I will investigate using the sqlite3.dll directly instead
of going through System.Data.SQLite. If there are significant performance
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
tables and indexes just by examining your classes.

2. If you modify your database schema, Prius will detect this at startup
and add, remove or update database tables and columns automatically
if you open a database with any previous version of the schema.

3. You can separate application logic from database logic (both written
in C# or VB)  and later if you want to move to a client/server solution
like MySQL or SQL Server you can translate the database logic into
stored procedures and you won't have to make any changes to the code
for the application logic.

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
and encapsulate the SQL statements in a SqLite specific DLL instead.

* If you want this package to create the SqLite database schema for
you, then you need to decorate your data contracts with attributes that
provide information about primary keys, indexes and unique constraints.
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

# Flexibility options

This package uses IoC.Modules to wire everything up. This means that you can swap
out any interface for your own application specific version and everything inside
this package will use your version.

The interfaces you can override are:

`IColumnTypeMapper` is responsible for mapping DbType to the SqLite data type.

`IQueryRunner` is responsible for submitting queries to the SqLite engine.

`ICommandProcessorFactory` is responsible for constructing an object that will 
handle a specific request. The default implementation uses System.Data.SQLite to 
execute SQL statements and a custom implementation for stored procedures.

`ISchemaEnumerator` is responsible for discovering the database schema that is
expected by the application. The default implementation will load and examine
all DLLs in the bin folder. When data contract classes are decorated with
special schema attributes these are used to define the database schema.

`ISchemaUpdater` is responsible for comparing the actual database schema against
the schema that the application was compiled for, and modifying the database
schema to match the application.
