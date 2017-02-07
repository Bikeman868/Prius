using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces.External;
using UsersTestApp.DataAccess;
using UsersTestApp.Integration;

namespace UsersTestApp
{
    [Package]
    public class Package: IPackage
    {
        public string Name { get { return "Prius users test app"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<IErrorReporter, ErrorReporter>(),
                    new IocRegistration().Init<IFactory, Factory>(),
                    new IocRegistration().Init<IDataAccessLayer, StoredProcedureDataAccessLayer>(),
                    //new IocRegistration().Init<IDataAccessLayer, SqlStatementsDataAccessLayer>(),
                };
            }
        }
    }
}
