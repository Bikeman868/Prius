using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Orm.Commands;
using Prius.Orm.Connections;
using Prius.Orm.Enumeration;

namespace Prius.Orm
{
    [Package]
    public class Package : IPackage
    {
        public string Name => "Prius ORM";
        public IList<IocRegistration> IocRegistrations { get; private set; }

        public Package()
        {
            IocRegistrations = new List<IocRegistration>
            {
                // Prius provides implementations for these interfaces
                new IocRegistration().Init<ICommandFactory, CommandFactory>(),
                new IocRegistration().Init<IConnectionFactory, ConnectionFactory>(),
                new IocRegistration().Init<IContextFactory, ContextFactory>(),
                new IocRegistration().Init<IDataEnumeratorFactory, DataEnumeratorFactory>(),
                new IocRegistration().Init<IMapper, Mapper>(),
                new IocRegistration().Init<IParameterFactory, ParameterFactory>(),
                new IocRegistration().Init<IRepositoryFactory, RepositoryFactory>(),
                new IocRegistration().Init<IEnumerableDataFactory, EnumerableDataFactory>(),
                new IocRegistration().Init<IAsyncEnumerableFactory, AsyncEnumerableFactory>(),

                // Prius needs the application to implement these interfaces
                new IocRegistration().Init<IFactory>(),
                new IocRegistration().Init<IErrorReporter>()
            };
        }
    }
}
