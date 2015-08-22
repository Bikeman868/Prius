using System;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.Commands
{
    public class DataReader : Disposable, IDataReader
    {
        private readonly IErrorReporter _errorReporter;

        private System.Data.SqlClient.SqlDataReader _reader;
        private bool _hasErrors;
        private Action _errorAction;
        private Action _closeAction;

        public DataReader(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
        }

        public IDataReader Initialize(System.Data.SqlClient.SqlDataReader reader, string dataShapeName, Action closeAction, Action errorAction)
        {
            _reader = reader;
            DataShapeName = dataShapeName;
            _closeAction = closeAction;
            _errorAction = errorAction;
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            if (_hasErrors && _errorAction != null) _errorAction();
            if (_closeAction != null) _closeAction();
            base.Dispose(destructor);
        }

        public string DataShapeName { get; private set; }

        public bool IsServerOffline { get { return false; } }

        public Exception ServerOfflineException { get { return null; } }

        public int FieldCount
        {
            get { return _reader.FieldCount; }
        }

        public object this[int fieldIndex]
        {
            get { return _reader[fieldIndex]; }
        }

        public object this[string fieldName]
        {
            get { return _reader[fieldName]; }
        }

        public string GetFieldName(int fieldIndex)
        {
            return _reader.GetName(fieldIndex);
        }

        public int GetFieldIndex(string fieldName)
        {
            fieldName = fieldName.ToLower();
            for (var fieldIndex = 0; fieldIndex < _reader.FieldCount; fieldIndex++)
                if (_reader.GetName(fieldIndex).ToLower() == fieldName) return fieldIndex;
            return -1;
        }

        public bool IsNull(int fieldIndex)
        {
            return _reader.IsDBNull(fieldIndex);
        }

        public bool Read()
        {
            try
            {
                return _reader.Read();
            }
            catch (Exception ex)
            {
                _errorReporter.ReportError(ex, "Failed to read SQL result data");
                _hasErrors = true;
                return false;
            }
        }

        public bool NextResult()
        {
            try
            {
                return _reader.NextResult();
            }
            catch (Exception ex)
            {
                _errorReporter.ReportError(ex, "Failed to read next SQL result set");
                _hasErrors = true;
                return false;
            }
        }

        public object Get(int fieldIndex, object defaultValue, Type type)
        {
            if (fieldIndex < 0 || _reader.IsDBNull(fieldIndex)) return defaultValue;
            if (type.IsNullable())
            {
                if (_reader.IsDBNull(fieldIndex)) return null;
                type = type.GetGenericArguments()[0];
            }
            if (type.IsEnum) type = typeof(int);
            try
            {
                return Convert.ChangeType(_reader[fieldIndex], type);
            }
            catch (Exception ex)
            {
                var msg = string.Format("DataReader failed to get field #{0} with type {1} as type {2}", fieldIndex, _reader.GetDataTypeName(fieldIndex), type.Name);
                throw new Exception(msg, ex);
            }
        }

        public T Get<T>(int fieldIndex, T defaultValue)
        {
            return (T)Get(fieldIndex, defaultValue, typeof(T));
        }

        public T Get<T>(string fieldName, T defaultValue)
        {
            return (T)Get(GetFieldIndex(fieldName), defaultValue, typeof(T));
        }

        public object Get(string fieldName, object defaultValue, Type type)
        {
            return Get(GetFieldIndex(fieldName), defaultValue, type);
        }
    }
}
