using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Mocks.Helper
{
    internal class DataReader : IDataReader
    {
        private IEnumerator<IMockedResultSet> _resultSetEnumerator;
        private IEnumerator<JObject> _rowEnumerator;

        public IEnumerable<IMockedResultSet> ResultSets { get; private set; }

        public IDataReader Initialize(string dataShapeName, IEnumerable<IMockedResultSet> resultSets)
        {
            DataShapeName = dataShapeName;
            ResultSets = resultSets;

            _resultSetEnumerator = resultSets.GetEnumerator();
            if (_resultSetEnumerator.MoveNext())
            {
                if (_resultSetEnumerator.Current != null && _resultSetEnumerator.Current.Data != null)
                    _rowEnumerator = _resultSetEnumerator.Current.Data.GetEnumerator();
            }

            return this;
        }

        public string DataShapeName { get; private set; }

        public int FieldCount
        {
            get
            {
                if (_resultSetEnumerator == null || _resultSetEnumerator.Current == null) return 0;
                var schema = _resultSetEnumerator.Current.Schema;
                return schema == null ? 0 : schema.Count();
            }
        }

        public bool IsServerOffline { get { return false; } }

        public Exception ServerOfflineException { get { return null; } }

        public object this[int fieldIndex]
        {
            get
            {
                var fieldName = GetFieldName(fieldIndex);
                return this[fieldName];
            }
        }

        public object this[string fieldName]
        {
            get
            {
                if (_rowEnumerator == null || _rowEnumerator.Current == null) return null;
                var field = _rowEnumerator.Current.GetValue(fieldName);
                return ((JValue)field).Value;
            }
        }

        public string GetFieldName(int fieldIndex)
        {
            if (fieldIndex < 0) return null;

            if (_resultSetEnumerator == null || _resultSetEnumerator.Current == null) return null;
            var schema = _resultSetEnumerator.Current.Schema;

            var property = schema.Skip(fieldIndex).FirstOrDefault();
            return property == null ? null : property.Name;
        }

        public int GetFieldIndex(string fieldName)
        {
            if (_resultSetEnumerator == null || _resultSetEnumerator.Current == null || _resultSetEnumerator.Current.Schema == null) return -1;
            var schema = _resultSetEnumerator.Current.Schema;

            var i = 0;
            foreach (var property in schema)
            {
                if (String.Equals(property.Name, fieldName, StringComparison.InvariantCultureIgnoreCase))
                    return i;
                i++;
            }
            return -1;
        }

        public bool IsNull(int fieldIndex)
        {
            var fieldName = GetFieldName(fieldIndex);
            if (_rowEnumerator == null || _rowEnumerator.Current == null) return true;
            var field = _rowEnumerator.Current.GetValue(fieldName);
            return field == null || field.Type == JTokenType.Null;
        }

        public bool Read()
        {
            return _rowEnumerator.MoveNext();
        }

        public bool NextResult()
        {
            var result = _resultSetEnumerator.MoveNext();
            _rowEnumerator = result ? _resultSetEnumerator.Current.Data.GetEnumerator() : null;
            return result;
        }

        public T Get<T>(int fieldIndex, T defaultValue)
        {
            var fieldName = GetFieldName(fieldIndex);
            if (fieldName == null) return defaultValue;

            if (_rowEnumerator == null || _rowEnumerator.Current == null) return defaultValue;
            var field = _rowEnumerator.Current.GetValue(fieldName) as JValue;
            return field == null ? defaultValue : field.Value<T>();
        }

        public T Get<T>(string fieldName, T defaultValue)
        {
            if (_rowEnumerator == null || _rowEnumerator.Current == null) return defaultValue;
            var field = _rowEnumerator.Current.GetValue(fieldName) as JValue;
            return field == null ? defaultValue : field.Value<T>();
        }

        public object Get(int fieldIndex, object defaultValue, Type type)
        {
            var fieldName = GetFieldName(fieldIndex);
            if (fieldName == null) return defaultValue;

            if (_rowEnumerator == null || _rowEnumerator.Current == null) return defaultValue;
            var field = _rowEnumerator.Current.GetValue(fieldName) as JValue;
            return field == null ? defaultValue : Convert.ChangeType(field.Value, type);
        }

        public bool IsReusable { get { return false; } }

        public bool IsDisposing { get { return false; } }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

}
