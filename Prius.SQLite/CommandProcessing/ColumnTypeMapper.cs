using System;
using System.Collections.Generic;
using System.Data;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.CommandProcessing
{
    /// <summary>
    /// SqLite has very few data types, and you can store any
    /// kind of data in any column regardless of its type.
    /// This class maps System.Data.DbType onto a SqLite type.
    /// This mapping only affects the way that data is sorted
    /// in this column.
    /// </summary>
    public class ColumnTypeMapper: IColumnTypeMapper
    {
        private readonly IDictionary<DbType, string> _dataTypeMap;

        public ColumnTypeMapper()
        {
            _dataTypeMap = new Dictionary<DbType, string>();
            _dataTypeMap[DbType.AnsiString] = "TEXT";
            _dataTypeMap[DbType.AnsiStringFixedLength] = "TEXT";
            _dataTypeMap[DbType.Binary] = "BLOB";
            _dataTypeMap[DbType.Boolean] = "NUMERIC";
            _dataTypeMap[DbType.Byte] = "INTEGER";
            _dataTypeMap[DbType.Currency] = "NUMERIC";
            _dataTypeMap[DbType.Date] = "TEXT";
            _dataTypeMap[DbType.DateTime] = "TEXT";
            _dataTypeMap[DbType.DateTime2] = "TEXT";
            _dataTypeMap[DbType.DateTimeOffset] = "TEXT";
            _dataTypeMap[DbType.Decimal] = "NUMERIC";
            _dataTypeMap[DbType.Double] = "REAL";
            _dataTypeMap[DbType.Guid] = "TEXT";
            _dataTypeMap[DbType.Int16] = "INTEGER";
            _dataTypeMap[DbType.Int32] = "INTEGER";
            _dataTypeMap[DbType.Int64] = "INTEGER";
            _dataTypeMap[DbType.Object] = "BLOB";
            _dataTypeMap[DbType.SByte] = "INTEGER";
            _dataTypeMap[DbType.Single] = "REAL";
            _dataTypeMap[DbType.String] = "TEXT";
            _dataTypeMap[DbType.StringFixedLength] = "TEXT";
            _dataTypeMap[DbType.Time] = "INTEGER";
            _dataTypeMap[DbType.UInt16] = "INTEGER";
            _dataTypeMap[DbType.UInt32] = "INTEGER";
            _dataTypeMap[DbType.UInt64] = "INTEGER";
            _dataTypeMap[DbType.VarNumeric] = "REAL";
            _dataTypeMap[DbType.Xml] = "TEXT";
        }

        public string MapToSqLite(DbType type)
        {
            return _dataTypeMap[type];
        }
    }
}
