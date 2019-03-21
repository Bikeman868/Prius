using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Prius.Orm.Commands;
using ParameterDirection = Prius.Contracts.Attributes.ParameterDirection;

namespace Prius.Tests
{
    [TestFixture]
    public class ParameterFactoryTests
    {
        private ParameterFactory _parameterFactory;

        [SetUp]
        public void Setup()
        {
            _parameterFactory = new ParameterFactory();
        }

        [Test]
        public void Should_allow_null_strings()
        {
            var parameter = _parameterFactory.Create("param1", (string)null, ParameterDirection.Input);

            Assert.AreEqual(SqlDbType.VarChar, parameter.DbType);
            Assert.AreEqual(ParameterDirection.Input, parameter.Direction);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(typeof(string), parameter.Type);
            Assert.IsNull(parameter.Value);
        }
    }
}
