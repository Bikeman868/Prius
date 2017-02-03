using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Orm.Commands;
using Prius.Orm.Connections;
using Prius.Orm.Enumeration;
using Prius.Orm.Results;

namespace Prius.Orm
{
    [Package]
    public class Package : IPackage
    {
        public string Name { get { return "Prius ORM"; } }
        public IList<IocRegistration> IocRegistrations { get; private set; }

        public Package()
        {
            IocRegistrations = new List<IocRegistration>
            {
                // Prius provides implementations for these interfaces
                new IocRegistration().Init<ICommandFactory, CommandFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IConnectionFactory, ConnectionFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IContextFactory, ContextFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IDataEnumeratorFactory, DataEnumeratorFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IDataReaderFactory, DataReaderFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IMapper, Mapper>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IParameterFactory, ParameterFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IRepositoryFactory, RepositoryFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IEnumerableDataFactory, EnumerableDataFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IAsyncEnumerableFactory, AsyncEnumerableFactory>(IocLifetime.SingleInstance),

                // Prius needs the application to implement these interfaces
                new IocRegistration().Init<IFactory>(IocLifetime.SingleInstance),
                new IocRegistration().Init<IErrorReporter>(IocLifetime.SingleInstance),
            };
        }
    }
}
