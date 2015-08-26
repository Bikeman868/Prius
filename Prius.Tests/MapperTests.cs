using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq.Modules;
using NUnit.Framework;
using Prius.Contracts.Interfaces;
using Prius.Orm.Enumeration;

namespace Prius.Tests
{
    [TestFixture]
    public class MapperTests: TestBase
    {
        private IMapper _mapper;
        private IDataReaderFactory _dataReaderFactory;

        [SetUp]
        public void Setup()
        {
            _dataReaderFactory = SetupMock<IDataReaderFactory>();
            _mapper = new Mapper(SetupMock<IFactory>(), SetupMock<IErrorReporter>());
        }

        public void Should_construct_data_contract_from_current_record()
        {
            //var dataReader = _dataReaderFactory.Create((System.Data.SqlClient.SqlDataReader)null, "");
            //dataReader.Read();
            //var dataContract = _mapper.Map<TestDataContract>(dataReader);
        }

        public void Should_fill_data_contract_from_current_record()
        {

        }

        public void Should_return_mapped_data_reader_from_reader()
        {

        }

        public class TestDataContract
        {

        }
    }
}
