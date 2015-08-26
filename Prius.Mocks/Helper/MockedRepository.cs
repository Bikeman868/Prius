using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Prius.Mocks.Helper
{
    public class MockedRepository : IMockedRepository
    {
        private Dictionary<string, IMockedStoredProcedure> _storedProcedures = new Dictionary<string, IMockedStoredProcedure>();
        private IMockedStoredProcedure _notMockedProcedure = new MockedStoredProcedure();

        public string Name { get; private set; }

        public MockedRepository(string name)
        {
            Name = name;
        }

        public IMockedStoredProcedure GetProcedure(string name)
        {
            IMockedStoredProcedure storedProcedure;
            if (_storedProcedures.TryGetValue(name.ToLower(), out storedProcedure))
                return storedProcedure;

            return _notMockedProcedure;
        }

        public void Add(string name, IMockedStoredProcedure mockedStoredProcedure)
        {
            _storedProcedures[name.ToLower()] = mockedStoredProcedure;
        }

        public void Add(string name, JArray data = null, int? rowsAffected = null, Func<JObject, bool> predicate = null, IEnumerable<JProperty> schema = null)
        {
            _storedProcedures[name.ToLower()] = new MockedStoredProcedure(data, rowsAffected, predicate, schema);
        }
    }
}
