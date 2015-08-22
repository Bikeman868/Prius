using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using Prius.Contracts.Interfaces;
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
                SetupMock<IDataReaderFactory>(),
                SetupMock<IRepositoryFactory>(),
                SetupMock<IErrorReporter>());
            _context = contextFactory.Create("TestRepository");
        }

        [Test]
        public void Should_execute_stored_procedure_and_return_objects()
        {
            using (var command = _commandFactory.CreateStoredProcedure("proc1"))
            {
                var asyncResult = _context.BeginExecuteEnumerable(command);
                using (var dataContracts = _context.EndExecuteEnumerable<TestDataContract>(asyncResult))
                {
                    var dataContractList = dataContracts.ToList();
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
