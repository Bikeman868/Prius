using System;
using Prius.Contracts.Attributes;

namespace Prius.Contracts.Interfaces.Commands
{
    public interface IParameter: IDisposable
    {
        string Name { get; }
        Type Type { get; set; }
        long Size { get; set; }
        System.Data.SqlDbType DbType { get; set; }
        ParameterDirection Direction { get; set; }
        Object Value { get; set; }
        Action<IParameter> StoreOutputValue { get; set; }
    }
}
