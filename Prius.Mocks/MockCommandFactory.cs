using System;
using System.Collections.Generic;
using System.Linq;
using Moq.Modules;
using Prius.Contracts.Interfaces;
using Moq;
using Prius.Contracts.Attributes;
using Prius.Contracts.Enumerations;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.Factory;

namespace Prius.Mocks
{
    public class MockCommandFactory : MockImplementationProvider<ICommandFactory>
    {
        protected override void SetupMock(IMockProducer mockProducer, Mock<ICommandFactory> mock)
        {
            mock.Setup(m =>
                m.CreateSql(
                    It.IsAny<string>(),
                    It.IsAny<int?>()))
                .Returns((string sql, int? timeoutSeconds) =>
                    new Command().Initialize(sql, CommandType.SQL, timeoutSeconds));

            mock.Setup(m =>
                m.CreateStoredProcedure(
                    It.IsAny<string>(),
                    It.IsAny<int?>()))
                .Returns((string procedureName, int? timeoutSeconds) =>
                    new Command().Initialize(procedureName, CommandType.StoredProcedure, timeoutSeconds));
        }

        private class Command: ICommand
        {
            private List<IParameter> _parameters = new List<IParameter>();

            public ICommand Initialize(string commandText, CommandType commandType, int? timeoutSeconds)
            {
                CommandText = commandText;
                CommandType = commandType;
                TimeoutSeconds = timeoutSeconds;

                return this;
            }

            public CommandType CommandType { get; private set; }

            public string CommandText { get; private set; }

            public int? TimeoutSeconds { get; set; }

            public IParameter AddParameter(string name)
            {
                var parameter = new Parameter().Initialize(name);
                _parameters.Add(parameter);
                return parameter;
            }

            public IParameter AddParameter(string name, System.Data.SqlDbType dbType, ParameterDirection direction)
            {
                var parameter = new Parameter().Initialize(name);
                parameter.DbType = dbType;
                _parameters.Add(parameter);
                return parameter;
            }

            public IParameter AddParameter<T>(string name, T value, ParameterDirection direction)
            {
                var parameter = new Parameter().Initialize(name);
                parameter.Value = value;
                _parameters.Add(parameter);
                return parameter;
            }

            public IParameter AddParameter<T>(string name, System.Data.SqlDbType dbType, T value, ParameterDirection direction)
            {
                var parameter = new Parameter().Initialize(name);
                parameter.DbType = dbType;
                parameter.Value = value;
                _parameters.Add(parameter);
                return parameter;
            }

            public void SetParameterValue<T>(string name, T value)
            {
                var parameter = _parameters.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
                if (parameter == null)
                {
                    AddParameter(name, value, ParameterDirection.Input);
                }
                else
                {
                    parameter.Value = value;
                }
            }

            public IEnumerable<IParameter> GetParameters()
            {
                return _parameters;
            }

            public void Lock()
            {
            }

            public void Unlock()
            {
            }

            public bool IsReusable { get { return true; } }

            public bool IsDisposing { get { return false; } }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class Parameter: IParameter
        {
            public IParameter Initialize(string name)
            {
                Name = name;
                Direction = ParameterDirection.Input;
                DbType = System.Data.SqlDbType.VarChar;
                Type = typeof(string);
                StoreOutputValue = p => { };

                return this;
            }

            public string Name { get; private set; }

            public Type Type { get; set; }

            public long Size { get; set; }

            public System.Data.SqlDbType DbType { get; set; }

            public ParameterDirection Direction { get; set; }

            public object Value { get; set; }

            public Action<IParameter> StoreOutputValue { get; set; }

            public bool IsReusable { get { return true; } }

            public bool IsDisposing { get { return false; } }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }

        }

    }
}
