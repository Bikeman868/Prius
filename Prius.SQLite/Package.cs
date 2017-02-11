using System.Collections.Generic;
using Ioc.Modules;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.SQLite.Interfaces;

namespace Prius.SQLite
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
                new IocRegistration().Init<ICommandProcessorFactory, CommandProcessing.CommandProcessorFactory>(),
                new IocRegistration().Init<IAdoQueryRunner, CommandProcessing.AdoQueryRunner>(),
                new IocRegistration().Init<IColumnTypeMapper, CommandProcessing.ColumnTypeMapper>(),
                new IocRegistration().Init<IDataReaderFactory, CommandProcessing.DataReaderFactory>(),
                new IocRegistration().Init<IParameterConverter, CommandProcessing.ParameterConverter>(),

                new IocRegistration().Init<ISchemaUpdater, Schema.SchemaUpdater>(),
                new IocRegistration().Init<ISchemaEnumerator, Schema.SchemaEnumerator>(),

                new IocRegistration().Init<IProcedureLibrary, Procedures.ProcedureLibrary>(),
                new IocRegistration().Init<IProcedureRunner, Procedures.Runner>(),
                new IocRegistration().Init<IParameterAccessor, Procedures.ParameterAccessor>(),

                new IocRegistration().Init<QueryBuilder.IQueryBuilder, QueryBuilder.QueryBuilder>(),

                new IocRegistration().Init<IFactory>(),
                new IocRegistration().Init<IErrorReporter>(),
                new IocRegistration().Init<IDataEnumeratorFactory>()
            };
        }
    }
}
