using System;
using Moq.Modules;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;
using Prius.Mocks.Helper;

namespace Prius.Mocks
{
    public class MockDataEnumeratorFactory : ConcreteImplementationProvider<IDataEnumeratorFactory>, IDataEnumeratorFactory
    {
        protected override IDataEnumeratorFactory GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public IDataEnumerator<T> Create<T>(IDataReader reader, Action closeAction = null, string dataSetName = null, IFactory<T> dataContractFactory = null) where T : class
        {
            var dataReader = reader as DataReader;
            if (dataReader != null)
                return new DataEnumerator<T>().Initialize(dataSetName, dataReader.Data);

            var data = new JArray();
            while (reader.Read())
            {
                var row = new JObject();
                for (var fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
                {
                    row[reader.GetFieldName(fieldIndex)] = new JValue(reader[fieldIndex]);
                }
                data.Add(row);
            }
            return new DataEnumerator<T>().Initialize(dataSetName, data);
        }
    }
}
