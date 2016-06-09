using System;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.Orm.Utility;

namespace Prius.Orm.Commands
{
    public class Parameter: Disposable, IParameter
    {
        public string Name { get; private set; }
        public Type Type { get; set; }
        public long Size { get; set; }
        public System.Data.SqlDbType DbType { get; set; }
        public ParameterDirection Direction { get; set; }
        public Object Value { get; set; }
        public Action<IParameter> StoreOutputValue { get; set; }

        public IParameter Initialize(
            string name,
            Type type,
            long size,
            System.Data.SqlDbType dbType,
            ParameterDirection direction,
            Object value,
            Action<IParameter> storeOutputValue = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ApplicationException("Parameter name can not be blank");
            Name = name[0] == '@' ? name.Substring(1) : name;
            Type = type;
            Size = size;
            DbType = dbType;
            Direction = direction;
            Value = value;
            StoreOutputValue = storeOutputValue ?? NoOutputValue;
            return this;
        }

        private void NoOutputValue(IParameter parameter)
        {
        }

        public static System.Data.SqlDbType DbTypeFrom<T>()
        {
            var type = typeof(T);
            var typeName = type.IsGenericType ? type.GetGenericArguments()[0].FullName : type.FullName;

            if (typeName == typeof(int).FullName) return System.Data.SqlDbType.Int;
            if (typeName == typeof(long).FullName) return System.Data.SqlDbType.BigInt;
            if (typeName == typeof(string).FullName) return System.Data.SqlDbType.VarChar;
            if (typeName == typeof(Guid).FullName) return System.Data.SqlDbType.UniqueIdentifier;
            if (typeName == typeof(Single).FullName) return System.Data.SqlDbType.Float;
            if (typeName == typeof(double).FullName) return System.Data.SqlDbType.Float;
            if (typeName == typeof(DateTime).FullName) return System.Data.SqlDbType.DateTime;
            if (typeName == typeof(byte[]).FullName) return System.Data.SqlDbType.Binary;
            if (typeName == typeof(System.Data.DataTable).FullName) return System.Data.SqlDbType.Structured;
            return System.Data.SqlDbType.VarChar;
        }
    }
}
