﻿using System;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.Contracts.Interfaces.Factory
{
    public interface IParameterFactory
    {
        IParameter Create(string name);
        IParameter Create(string name, System.Data.SqlDbType dbType, ParameterDirection direction = ParameterDirection.Output);
        IParameter Create<T>(string name, T value, ParameterDirection direction = ParameterDirection.Input);
        IParameter Create<T>(string name, T value, System.Data.SqlDbType dbType, ParameterDirection direction = ParameterDirection.Input, Action<IParameter> storeOutputValue = null);
        IParameter Create<T>(string name, T value, System.Data.SqlDbType dbType, int size, ParameterDirection direction, Action<IParameter> storeOutputValue = null);
    }
}
