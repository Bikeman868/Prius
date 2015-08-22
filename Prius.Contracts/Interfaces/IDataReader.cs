using System;

namespace Prius.Contracts.Interfaces
{
    public interface IDataReader : IDisposable
    {
        string DataShapeName { get; }
        int FieldCount { get; }
        bool IsServerOffline { get; }
        Exception ServerOfflineException { get; }
        object this[int fieldIndex] { get; }
        object this[string fieldName] { get; }

        string GetFieldName(int fieldIndex);
        int GetFieldIndex(string fieldName);
        bool IsNull(int fieldIndex);

        bool Read();
        bool NextResult();
        T Get<T>(int fieldIndex, T defaultValue = default(T));
        T Get<T>(string fieldName, T defaultValue = default(T));
        object Get(int fieldIndex, object defaultValue, Type type);
    }
}
