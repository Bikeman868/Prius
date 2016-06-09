using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Connections;
using Prius.Contracts.Interfaces.External;
using Prius.Contracts.Interfaces.Factory;
using Prius.Mocks;
using Prius.Mocks.Helper;
using Prius.Orm.Connections;
using Prius.Orm.Enumeration;

namespace Prius.Tests
{
    [TestFixture]
    public class ContextFactoryTests: TestBase
    {
        private IContext _context;
        private ICommandFactory _commandFactory;

        [SetUp]
        public void Setup()
        {
            _commandFactory = SetupMock<ICommandFactory>();

            _context = new Context(
                SetupMock<IDataEnumeratorFactory>(),
                SetupMock<IErrorReporter>(),
                SetupMock<IRepository>());

            var mockedRepository = new MockedRepository("MyData");
            mockedRepository.Add("proc1", JArray.Parse("[{id:10,name:\"name1\"},{id:20,name:\"name2\"}]"));

            var mockConnectionFactory = GetMock<MockConnectionFactory, IConnectionFactory>();
            mockConnectionFactory.MockedRepository = mockedRepository;
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

                    Assert.AreEqual(2, dataContractList.Count);

                    Assert.AreEqual(10, dataContractList[0].Id);
                    Assert.AreEqual("name1", dataContractList[0].Name);

                    Assert.AreEqual(20, dataContractList[1].Id);
                    Assert.AreEqual("name2", dataContractList[1].Name);
                }
            }
        }

        [Test]
        public void Should_execute_stored_procedure_asynchronously_and_return_objects()
        {
            var command = _commandFactory.CreateStoredProcedure("proc1");
            var asyncResult = _context.BeginExecuteEnumerable(command);

            IAsyncEnumerableFactory asyncEnumerableFactory = new AsyncEnumerableFactory();
            using (var asyncEnumerable = asyncEnumerableFactory.Create<TestDataContract>(_context, command, asyncResult))
            {
                asyncEnumerable.AsyncWaitHandle.WaitOne();
                using (var dataContracts = asyncEnumerable.GetResults())
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

        [Test]
        public void Should_execute_stored_procedure_and_return_a_reader()
        {
            using (var command = _commandFactory.CreateStoredProcedure("proc1"))
            {
                var asyncResult = _context.BeginExecuteReader(command);
                using (var reader = _context.EndExecuteReader(asyncResult))
                {
                    Assert.IsTrue(reader.Read());

                    Assert.AreEqual(10, reader.Get<int>(0));
                    Assert.AreEqual("name1", reader.Get<string>(1));

                    Assert.IsTrue(reader.Read());

                    Assert.AreEqual(20, reader.Get<int>(0));
                    Assert.AreEqual("name2", reader.Get<string>(1));

                    Assert.IsFalse(reader.Read());
                }
            }
        }

        [Test]
        public void Should_execute_stored_procedure_and_return_scalar()
        {
            using (var command = _commandFactory.CreateStoredProcedure("proc1"))
            {
                var asyncResult = _context.BeginExecuteScalar(command);
                Assert.AreEqual(10, _context.EndExecuteScalar<int>(asyncResult));
            }
        }

        [Test]
        public void Should_execute_stored_procedure_and_return_rows_affected()
        {
            using (var command = _commandFactory.CreateStoredProcedure("proc1"))
            {
                var asyncResult = _context.BeginExecuteNonQuery(command);
                Assert.AreEqual(2, _context.EndExecuteNonQuery(asyncResult));
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
