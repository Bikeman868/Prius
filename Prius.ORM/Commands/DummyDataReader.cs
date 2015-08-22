using System;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.Commands
{
    public class DummyDataReader : Disposable, IDataReader
    {
        private Exception _serverOfflineException;

        public DummyDataReader()
        {
        }

        public IDataReader Initialize(Exception exception)
        {
            _serverOfflineException = exception;
            return this;
        }

        public string DataShapeName { get { return ""; } }
        public int FieldCount { get { return 0; } }
        public object this[int fieldIndex] { get { return null; } }
        public object this[string fieldName] { get { return null; } }
        
        public string GetFieldName(int fieldIndex)
        {
            return "Field" + fieldIndex;
        }

        public int GetFieldIndex(string fieldName)
        {
            return 0;
        }

        public bool IsServerOffline { get { return true; } }

        public Exception ServerOfflineException { get { return _serverOfflineException; } }

        public bool IsNull(int fieldIndex)
        {
            return false;
        }

        public bool Read()
        {
            return false;
        }

        public bool NextResult()
        {
            return false;
        }

        public T Get<T>(int fieldIndex, T defaultValue)
        {
            return defaultValue;
        }

        public T Get<T>(string fieldName, T defaultValue)
        {
            return defaultValue;
        }

        public object Get(int fieldInfex, object defaultValue, Type type)
        {
            return defaultValue;
        }

        public object Get(string fieldName, object defaultValue, Type type)
        {
            return defaultValue;
        }

    }
}
