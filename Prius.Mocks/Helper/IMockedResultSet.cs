using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Prius.Mocks.Helper
{
    public interface IMockedResultSet
    {
        IEnumerable<JObject> Data { get; }
        IEnumerable<JProperty> Schema { get; }
    }
}
