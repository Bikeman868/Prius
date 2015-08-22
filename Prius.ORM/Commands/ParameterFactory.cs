using System;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;

namespace Prius.Orm.Commands
{
    public class ParameterFactory: IParameterFactory
    {
        public IParameterFactory Initialize()
        {
            return this;
        }

        public IParameter Create(string name)
        {
            return new Parameter().Initialize(name, typeof(object), 0, Parameter.DbTypeFrom<object>(), ParameterDirection.Input, null);
        }

        public IParameter Create(string name, System.Data.SqlDbType dbType, ParameterDirection direction)
        {
            return new Parameter().Initialize(name, typeof(object), 0, dbType, direction, null);
        }

        public IParameter Create<T>(string name, T value, ParameterDirection direction)
        {
            return new Parameter().Initialize(name, typeof(T), 0, Parameter.DbTypeFrom<T>(), direction, value);
        }

        public IParameter Create<T>(
            string name, 
            T value, 
            System.Data.SqlDbType dbType,
            ParameterDirection direction,
            Action<IParameter> storeOutputValue)
        {
            return new Parameter().Initialize(name, typeof(T), 0, dbType, direction, value, storeOutputValue);
        }

        public IParameter Create<T>(
            string name, 
            T value, 
            System.Data.SqlDbType dbType, 
            int size, 
            ParameterDirection direction,
            Action<IParameter> storeOutputValue)
        {
            return new Parameter().Initialize(name, typeof(T), size, dbType, direction, value, storeOutputValue);
        }

    }
}
