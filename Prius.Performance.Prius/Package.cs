using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces.External;
using Prius.Performance.Prius.Integration;
using Prius.Performance.Prius.Model;
using Prius.Performance.Shared;

namespace Prius.Performance.Prius
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "Prius performance test prius integration"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<IErrorReporter, ErrorReporter>(),
                    new IocRegistration().Init<IFactory, Factory>(),
                    new IocRegistration().Init<IDataAccessLayer, DataAccessLayer>(),
                    new IocRegistration().Init<ICustomer, Customer>(IocLifetime.MultiInstance),
                    new IocRegistration().Init<IOrder, Order>(IocLifetime.MultiInstance)
                };
            }
        }
    }
}
