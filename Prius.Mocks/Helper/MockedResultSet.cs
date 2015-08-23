using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Prius.Mocks.Helper
{
    public class MockedResultSet: IMockedResultSet
    {
        public IEnumerable<JObject> Data { get; private set; }
        public IEnumerable<JProperty> Schema { get; private set; }

        public MockedResultSet(JArray data, Func<JObject, bool> predicate = null, IEnumerable<JProperty> schema = null)
        {
            if (data == null)
            {
                Data = new JObject[0];
            }
            else
            {
                Data = data.Children().Cast<JObject>();
                if (predicate != null) Data = Data.Where(predicate);
            }

            if (schema == null && Data != null)
            {
                var list = Data.ToList();
                var first = list.FirstOrDefault();
                if (first != null) schema = first.Properties();
                Data = list;
            }
            Schema = schema;
        }

    }
}
