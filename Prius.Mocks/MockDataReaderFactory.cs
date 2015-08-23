using System;
using Moq;
using Moq.Modules;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;
using Prius.Mocks.Helper;
using DataReader = Prius.Mocks.Helper.DataReader;

namespace Prius.Mocks
{
    public class MockDataReaderFactory: MockImplementationProvider<IDataReaderFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<IDataReaderFactory> mock)
        {
            var emptyResults = new[] {new MockedResultSet(null)};

            mock.Setup(f => f.Create(It.IsAny<Exception>()))
                .Returns((Exception e) => new DataReader().Initialize(null, emptyResults));

            mock.Setup(f => f.Create(It.IsAny<MySql.Data.MySqlClient.MySqlDataReader>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Returns((MySql.Data.MySqlClient.MySqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction) => new DataReader().Initialize(dataShapeName, emptyResults));

            mock.Setup(f => f.Create(It.IsAny<Npgsql.NpgsqlDataReader>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Returns((Npgsql.NpgsqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction) => new DataReader().Initialize(dataShapeName, emptyResults));

            mock.Setup(f => f.Create(It.IsAny<System.Data.SqlClient.SqlDataReader>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Returns((System.Data.SqlClient.SqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction) => new DataReader().Initialize(dataShapeName, emptyResults));
        }
    }
}
