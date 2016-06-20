using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces.External;

namespace Prius.Performance.Prius.Integration
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
                    new IocRegistration().Init<IErrorReporter, ErrorReporter>(IocLifetime.SingleInstance),
                    new IocRegistration().Init<IFactory, Factory>(IocLifetime.SingleInstance)
                };
            }
        }
    }
}
