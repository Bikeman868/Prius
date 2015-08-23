using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Prius.Contracts.Interfaces;

namespace Prius.Mocks.Helper
{
    internal class DataReader : IDataReader
    {
        private int _rowNumber;
        private List<JProperty> _schema;

        public JArray Data { get; private set; }

        public IDataReader Initialize(string dataShapeName, JArray data)
        {
            DataShapeName = dataShapeName;
            Data = data;
            _rowNumber = -1;

            if (data != null && data.Count > 0)
            {
                if (data.First is JObject)
                    _schema = (data.First as JObject).Properties().ToList();
            }

            return this;
        }

        public string DataShapeName { get; private set; }

        public int FieldCount
        {
            get
            {
                return _schema == null ? 0 : _schema.Count;
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
                return ((Data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value;
            }
        }

        public string GetFieldName(int fieldIndex)
        {
            if (fieldIndex < 0) return null;

            var property = _schema.Skip(fieldIndex).FirstOrDefault();
            if (property == null) return null;
            return property.Name;
        }

        public int GetFieldIndex(string fieldName)
        {
            if (_schema == null) return -1;

            for (var i = 0; i < _schema.Count; i++)
            {
                if (_schema[i].Name.ToLower() == fieldName.ToLower())
                    return i;
            }
            return -1;
        }

        public bool IsNull(int fieldIndex)
        {
            var fieldName = GetFieldName(fieldIndex);
            var value = (Data[_rowNumber] as JObject).GetValue(fieldName) as JValue;
            if (value == null) return true;
            return value.Value == null;
        }

        public bool Read()
        {
            if (_rowNumber == Data.Count - 1)
                return false;

            _rowNumber++;
            return true;
        }

        public bool NextResult()
        {
            return Read();
        }

        public T Get<T>(int fieldIndex, T defaultValue)
        {
            var fieldName = GetFieldName(fieldIndex);
            if (fieldName == null) return defaultValue;

            return ((Data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value<T>();
        }

        public T Get<T>(string fieldName, T defaultValue)
        {
            return ((Data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value<T>();
        }

        public object Get(int fieldIndex, object defaultValue, Type type)
        {
            var fieldName = GetFieldName(fieldIndex);
            if (fieldName == null) return defaultValue;

            return ((Data[_rowNumber] as JObject).GetValue(fieldName) as JValue).Value;
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
