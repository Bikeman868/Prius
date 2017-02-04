using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;

namespace Prius.SqLite
{
    [Package]
    public class Package : IPackage
    {
        public string Name { get { return "Prius SQLite driver"; } }
        public IList<IocRegistration> IocRegistrations { get; private set; }

        public Package()
        {
            IocRegistrations = new List<IocRegistration>
            {
                new IocRegistration().Init<ICommandProcessorFactory, CommandProcessorFactory>(),

                new IocRegistration().Init<IFactory>(),
                new IocRegistration().Init<IErrorReporter>(),
                new IocRegistration().Init<IDataEnumeratorFactory>()
            };
        }
    }
}
