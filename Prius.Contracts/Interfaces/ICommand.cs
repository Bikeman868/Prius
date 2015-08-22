using System;
using System.Collections.Generic;
using Prius.Contracts.Enumerations;
using Prius.Contracts.Attributes;

namespace Prius.Contracts.Interfaces
{
    public interface ICommand : IDisposable
    {
        CommandType CommandType { get; }
        string CommandText { get; }
        int TimeoutSeconds { get; set; }

        IParameter AddParameter(string name);
        IParameter AddParameter(string name, System.Data.SqlDbType dbType, ParameterDirection direction = ParameterDirection.Output);
        IParameter AddParameter<T>(string name, T value, ParameterDirection direction = ParameterDirection.Input);
        IParameter AddParameter<T>(string name, System.Data.SqlDbType dbType, T value, ParameterDirection direction = ParameterDirection.Input);

        void SetParameterValue<T>(string name, T value);
        IEnumerable<IParameter> GetParameters();

        void Lock();
        void Unlock();
    }

}
