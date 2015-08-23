using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Moq.Modules;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Prius.Contracts.Interfaces;
using Prius.Mocks;

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

            var mockContextFactory = GetMock<MockContextFactory, IContextFactory>();

            // Mock data returned by the sp_GetUsers stored procedure
            var proc1Data = new JArray();
            proc1Data.Add(JObject.Parse("{userId:1,userName:\"martin\"}"));
            proc1Data.Add(JObject.Parse("{userId:2,userName:\"wilson\"}"));
            proc1Data.Add(JObject.Parse("{userId:3,userName:\"daisy\"}"));
            proc1Data.Add(JObject.Parse("{userId:4,userName:\"nancy\"}"));
            mockContextFactory.ClearMockData("sp_GetUsers");
            mockContextFactory.AddMockData("sp_GetUsers", proc1Data);

            // Mock data returned by the sp_GetProducts stored procedure
            var proc2Data = new JArray();
            proc2Data.Add(JObject.Parse("{productId:1,sku:\"abc45324\"}"));
            proc2Data.Add(JObject.Parse("{productId:2,sku:\"gtf34355\"}"));
            proc2Data.Add(JObject.Parse("{productId:3,sku:\"iuo76573\"}"));
            proc2Data.Add(JObject.Parse("{productId:4,sku:\"zer76576\"}"));
            mockContextFactory.ClearMockData("sp_GetProducts");
            mockContextFactory.AddMockData("sp_GetProducts", proc2Data);

            // Define a function that will filter the data according to the parameters passed to the sproc
            mockContextFactory.SetFilter("sp_GetUsers", UsersFilter);

            // Define what the database does on ExecuteScalar reqests
            mockContextFactory.SetScalar("sp_CreateUser", parameters => 5);

            // Define what the database does on ExecuteNonQuery reqests
            mockContextFactory.SetNonQuery("sp_DeleteUser", parameters => 1);
        }

        private JArray UsersFilter(JArray data, List<IParameter> commandParameters)
        {
            var userIdParameter = (IParameter)null;
            if (commandParameters != null)
                userIdParameter = commandParameters.FirstOrDefault(p => string.Compare(p.Name, "userid", StringComparison.InvariantCultureIgnoreCase) == 0);

            if (userIdParameter == null)
                return data;

            return data
                .Cast<JObject>()
                .Where(o => o["userId"].Value<int>() == (int)userIdParameter.Value)
                .Aggregate(new JArray(), (a, o) =>
                    {
                        a.Add(o);
                        return a;
                    });
        }

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
