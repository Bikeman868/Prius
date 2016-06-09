using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Mocks.Helper
{
    public class MockedStoredProcedure : IMockedStoredProcedure
    {
        protected long _rowsAffected;
        protected object _scalar;
        protected IMockedResultSet _results;

        public MockedStoredProcedure(JArray data = null, int? rowsAffected = null, Func<JObject, bool> predicate = null, IEnumerable<JProperty> schema = null)
        {
           SetData(data, rowsAffected, predicate, schema);
        }

        public virtual IEnumerable<IMockedResultSet> Query(ICommand command)
        {
            return _results == null ? new IMockedResultSet[0] : new [] {_results};
        }

        public virtual long NonQuery(ICommand command)
        {
            return _rowsAffected;
        }

        public virtual T Scalar<T>(ICommand command)
        {
            if (_scalar == null)
                return default(T);
            return (T)Convert.ChangeType(_scalar, typeof (T));
        }

        protected void SetData(JArray data = null, int? rowsAffected = null, Func<JObject, bool> predicate = null, IEnumerable<JProperty> schema = null)
        {
            _results = new MockedResultSet(data, predicate, schema);
            var firstRow = _results.Data.FirstOrDefault();
            if (firstRow != null)
            {
                var firstColumn = firstRow.Properties().FirstOrDefault();
                if (firstColumn != null)
                    _scalar = firstColumn.Value;
            }
            _rowsAffected = rowsAffected ?? _results.Data.LongCount();
        }

        protected T GetParameterValue<T>(ICommand command, string parameterName, T defaultValue)
        {

            var parameters = command.GetParameters();
            if (parameters == null) return defaultValue;

            var parameter = parameters.FirstOrDefault(p => string.Compare(p.Name, parameterName, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (parameter == null) return defaultValue;

            return (T)Convert.ChangeType(parameter.Value, typeof(T));
        }
    }
}
