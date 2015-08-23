using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Moq.Modules;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Prius.Contracts.Interfaces;
using Prius.Mocks;
using Prius.Mocks.Helper;

namespace Prius.Tests
{
    /// <summary>
    /// This is a slightly unusual unit test, because it tests the mocks that applications
    /// will use in their unit tests.
    /// </summary>
    public class MockContextFactoryTests: TestBase
    {
        private IContextFactory _contextFactory;
        private ICommandFactory _commandFactory;

        [SetUp]
        public void Setup()
        {
            _commandFactory = SetupMock<ICommandFactory>();
            _contextFactory = SetupMock<IContextFactory>();

            var usersTable = new JArray();
            usersTable.Add(JObject.Parse("{userId:1,userName:\"martin\"}"));
            usersTable.Add(JObject.Parse("{userId:2,userName:\"wilson\"}"));
            usersTable.Add(JObject.Parse("{userId:3,userName:\"daisy\"}"));
            usersTable.Add(JObject.Parse("{userId:4,userName:\"nancy\"}"));

            var productTable = new JArray();
            productTable.Add(JObject.Parse("{productId:1,sku:\"abc45324\"}"));
            productTable.Add(JObject.Parse("{productId:2,sku:\"gtf34355\"}"));
            productTable.Add(JObject.Parse("{productId:3,sku:\"iuo76573\"}"));
            productTable.Add(JObject.Parse("{productId:4,sku:\"zer76576\"}"));

            var mockedRepository = new MockedRepository("MyData");
            mockedRepository.Add("sp_GetAllUsers", usersTable);
            mockedRepository.Add("sp_GetUser", new GetUserProcedure(usersTable));
            mockedRepository.Add("sp_CreateUser", new CraeteUserProcedure(usersTable));
            mockedRepository.Add("sp_DeleteUser", new DeleteUserProcedure(usersTable));

            var mockContextFactory = GetMock<MockContextFactory, IContextFactory>();
            mockContextFactory.MockedRepository  = mockedRepository;
        }

        #region Stored procedure mocks

        private class GetUserProcedure: MockedStoredProcedure
        {
            private JArray _userData;

            public GetUserProcedure(JArray userData)
            {
                _userData = userData;
            }

            public override IEnumerable<IMockedResultSet> Query(ICommand command)
            {
                Execute(command);
                return base.Query(command);
            }

            public long NonQuery(ICommand command)
            {
                Execute(command);
                return base.NonQuery(command);
            }

            public T Scalar<T>(ICommand command)
            {
                Execute(command);
                return base.Scalar<T>(command);
            }

            private void Execute(ICommand command)
            {
                var userIdParameter = (IParameter) null;
                var parameters = command.GetParameters();
                if (parameters != null)
                    userIdParameter = parameters.FirstOrDefault(p => string.Compare(p.Name, "userid", StringComparison.InvariantCultureIgnoreCase) == 0);

                if (userIdParameter == null)
                    SetData(_userData);
                else
                {
                    SetData(_userData, null, o => o["userId"].Value<int>() == (int)userIdParameter.Value);
                }
            }
        }

        private class CraeteUserProcedure : MockedStoredProcedure
        {
            private JArray _userData;

            public CraeteUserProcedure(JArray userData)
            {
                _userData = userData;
            }

            public override long NonQuery(ICommand command)
            {
                // You can add a new row to the data here for a full simulation of what the database does
                return 1;
            }
        }

        private class DeleteUserProcedure : MockedStoredProcedure
        {
            private JArray _userData;

            public DeleteUserProcedure(JArray userData)
            {
                _userData = userData;
            }

            public override long NonQuery(ICommand command)
            {
                // You can delete a new row to the data here for a full simulation of what the database does
                return 1;
            }
        }

        #endregion

        [Test]
        public void Should_execute_stored_procedure_and_return_objects()
        {
            using (var context = _contextFactory.Create("myData"))
            {
                using (var command = _commandFactory.CreateStoredProcedure("sp_GetUsers"))
                {
                    command.AddParameter("userId", 3);
                    using (var data = context.ExecuteEnumerable<UserDataContract>(command))
                    {
                        Assert.IsNotNull(data);
                        var user = data.FirstOrDefault();
                        Assert.IsNotNull(user);
                        Assert.AreEqual(3, user.UserId);
                        Assert.AreEqual("daisy", user.UserName);
                    }
                }
            }
        }

        [Test]
        public void Should_execute_stored_procedure_and_return_reader()
        {
            using (var context = _contextFactory.Create("myData"))
            {
                using (var command = _commandFactory.CreateStoredProcedure("sp_GetUsers"))
                {
                    command.AddParameter("userId", 2);
                    using (var reader = context.ExecuteReader(command))
                    {
                        Assert.IsTrue(reader.Read());

                        Assert.AreEqual(2, reader.Get<int>("userId"));
                        Assert.AreEqual("wilson", reader.Get<string>("userName"));

                        Assert.IsFalse(reader.Read());
                    }
                }
            }
        }

        [Test]
        public void Should_execute_stored_procedure_and_return_scalar()
        {
            using (var context = _contextFactory.Create("myData"))
            {
                using (var command = _commandFactory.CreateStoredProcedure("sp_CreateUser"))
                {
                    command.AddParameter("userName", "fred");
                    var userId = context.ExecuteScalar<int>(command);
                    Assert.AreEqual(5, userId);
                }
            }
        }

        [Test]
        public void Should_execute_stored_procedure_and_return_rows_affected()
        {
            using (var context = _contextFactory.Create("myData"))
            {
                using (var command = _commandFactory.CreateStoredProcedure("sp_DeleteUser"))
                {
                    command.AddParameter("userId", 1);
                    var rowsAffected = context.ExecuteNonQuery(command);
                    Assert.AreEqual(1, rowsAffected);
                }
            }
        }

        public class UserDataContract
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
        }
    }
}
