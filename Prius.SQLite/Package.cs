using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.SqLite.CommandProcessing;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Schema;

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
                new IocRegistration().Init<ISchemaUpdater, SchemaUpdater>(),
                new IocRegistration().Init<IQueryRunner, QueryRunner>(),
                new IocRegistration().Init<IColumnTypeMapper, ColumnTypeMapper>(),
                new IocRegistration().Init<ISchemaEnumerator, SchemaEnumerator>(),

                new IocRegistration().Init<IFactory>(),
                new IocRegistration().Init<IErrorReporter>(),
                new IocRegistration().Init<IDataEnumeratorFactory>()
            };
        }
    }
}
