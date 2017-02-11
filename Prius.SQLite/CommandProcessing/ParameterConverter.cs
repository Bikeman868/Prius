using System;
using System.Data.SQLite;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces.Commands;
using Prius.SQLite.Interfaces;

namespace Prius.SQLite.CommandProcessing
{
    /// <summary>
    /// This class takes Prius command parameters and sets them into SQLite
    /// commands. This version only has support for the ADO.Net driver in
    /// System.Data.SQLite
    /// </summary>
    internal class ParameterConverter : IParameterConverter
    {
        public void AddParameter(SQLiteCommand command, IParameter parameter)
        {
            SQLiteParameter sqLiteParameter;
            switch (parameter.Direction)
            {
                case ParameterDirection.Input:
                    command.Parameters.AddWithValue("@" + parameter.Name, parameter.Value);
                    break;
                case ParameterDirection.InputOutput:
                    sqLiteParameter = command.Parameters.AddWithValue("@" + parameter.Name, parameter.Value);
                    sqLiteParameter.Direction = System.Data.ParameterDirection.InputOutput;
                    parameter.StoreOutputValue = p => p.Value = sqLiteParameter.Value;
                    break;
                case ParameterDirection.Output:
                    sqLiteParameter = command.Parameters.Add("@" + parameter.Name, ToSQLiteDbType(parameter.DbType), (int)parameter.Size);
                    sqLiteParameter.Direction = System.Data.ParameterDirection.Output;
                    parameter.StoreOutputValue = p => p.Value = sqLiteParameter.Value;
                    break;
                case ParameterDirection.ReturnValue:
                    sqLiteParameter = command.Parameters.Add("@" + parameter.Name, ToSQLiteDbType(parameter.DbType), (int)parameter.Size);
                    sqLiteParameter.Direction = System.Data.ParameterDirection.ReturnValue;
                    parameter.StoreOutputValue = p => p.Value = sqLiteParameter.Value;
                    break;
            }
        }

        private System.Data.DbType ToSQLiteDbType(System.Data.SqlDbType dbType)
        {
            switch (dbType)
            {
                case System.Data.SqlDbType.BigInt:
                    return System.Data.DbType.Int64;
                case System.Data.SqlDbType.Binary:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.Bit:
                    return System.Data.DbType.Boolean;
                case System.Data.SqlDbType.Char:
                    return System.Data.DbType.Byte;
                case System.Data.SqlDbType.Date:
                    return System.Data.DbType.Date;
                case System.Data.SqlDbType.DateTime:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.DateTime2:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.DateTimeOffset:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.Decimal:
                    return System.Data.DbType.Decimal;
                case System.Data.SqlDbType.Float:
                    return System.Data.DbType.Single;
                case System.Data.SqlDbType.Image:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.Int:
                    return System.Data.DbType.UInt32;
                case System.Data.SqlDbType.Money:
                    return System.Data.DbType.Decimal;
                case System.Data.SqlDbType.NChar:
                    return System.Data.DbType.UInt32;
                case System.Data.SqlDbType.NText:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.NVarChar:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.Real:
                    return System.Data.DbType.Double;
                case System.Data.SqlDbType.SmallDateTime:
                    return System.Data.DbType.DateTime;
                case System.Data.SqlDbType.SmallInt:
                    return System.Data.DbType.Int16;
                case System.Data.SqlDbType.SmallMoney:
                    return System.Data.DbType.Decimal;
                case System.Data.SqlDbType.Structured:
                    return System.Data.DbType.Object;
                case System.Data.SqlDbType.Text:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.Time:
                    return System.Data.DbType.Time;
                case System.Data.SqlDbType.Timestamp:
                    return System.Data.DbType.DateTimeOffset;
                case System.Data.SqlDbType.TinyInt:
                    return System.Data.DbType.Int16;
                case System.Data.SqlDbType.Udt:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.UniqueIdentifier:
                    return System.Data.DbType.Guid;
                case System.Data.SqlDbType.VarBinary:
                    return System.Data.DbType.Binary;
                case System.Data.SqlDbType.VarChar:
                    return System.Data.DbType.String;
                case System.Data.SqlDbType.Variant:
                    return System.Data.DbType.Object;
                case System.Data.SqlDbType.Xml:
                    return System.Data.DbType.Xml;
            }

            return System.Data.DbType.String;
        }
    }
}
