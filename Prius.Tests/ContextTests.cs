using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Prius.Contracts.Interfaces;
using Prius.Mocks;
using Prius.Orm.Connections;

namespace Prius.Tests
{
    [TestFixture]
    public class ContextTests: TestBase
    {
        private IContext _context;
        private ICommandFactory _commandFactory;

        [SetUp]
        public void Setup()
        {
            _commandFactory = SetupMock<ICommandFactory>();

            var contextFactory = new ContextFactory(
                SetupMock<IDataEnumeratorFactory>(),
                SetupMock<IRepositoryFactory>(),
                SetupMock<IErrorReporter>());
            _context = contextFactory.Create("TestRepository");
        }

        [Test]
        public void Should_execute_stored_procedure_and_return_objects()
        {
            var mockConnectionFactory = GetMock<MockConnectionFactory, IConnectionFactory>();
            
            using (var command = _commandFactory.CreateStoredProcedure("proc1"))
            {
                mockConnectionFactory.AddQueryFunction(command, c => JArray.Parse("[{id:10,name:\"name1\"},{id:20,name:\"name2\"}]"));
                
                var asyncResult = _context.BeginExecuteEnumerable(command);
                using (var dataContracts = _context.EndExecuteEnumerable<TestDataContract>(asyncResult))
                {
                    var dataContractList = dataContracts.ToList();

                    Assert.AreEqual(2, dataContractList.Count);

                    Assert.AreEqual(10, dataContractList[0].Id);
                    Assert.AreEqual("name1", dataContractList[0].Name);

                    Assert.AreEqual(20, dataContractList[1].Id);
                    Assert.AreEqual("name2", dataContractList[1].Name);
                }
            }
        }

        private class TestDataContract
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
