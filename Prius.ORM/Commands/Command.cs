using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Prius.Contracts.Attributes;
using Prius.Contracts.Enumerations;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.Commands
{
    public class Command : Disposable, ICommand
    {
        private readonly IParameterFactory _parameterFactory;

        private List<IParameter> _parameters = new List<IParameter>();

        public CommandType CommandType { get; private set; }
        public string CommandText { get; private set; }
        public int TimeoutSeconds { get; set; }

        public Command(IParameterFactory parameterFactory)
        {
            _parameterFactory = parameterFactory;
        }

        public ICommand Initialize(CommandType commandType, string commandText, int timeoutSeconds)
        {
            CommandType = commandType;
            CommandText = commandText;
            TimeoutSeconds = timeoutSeconds;
            return this;
        }

        protected override void Dispose(bool destructor)
        {
            if (!destructor)
            {
                foreach (var parameter in _parameters) parameter.Dispose();
            }
        }

        public IParameter AddParameter(string name)
        {
            var parameter = _parameterFactory.Create(name);
            _parameters.Add(parameter);
            return parameter;
        }

        public IParameter AddParameter(string name, System.Data.SqlDbType dbType, ParameterDirection direction)
        {
            var parameter = _parameterFactory.Create(name, dbType, direction);
            _parameters.Add(parameter);
            return parameter;
        }

        public IParameter AddParameter<T>(string name, T value, ParameterDirection direction)
        {
            var parameter = _parameterFactory.Create<T>(name, value, direction);
            _parameters.Add(parameter);
            return parameter;
        }

        public IParameter AddParameter<T>(string name, System.Data.SqlDbType dbType, T value, ParameterDirection direction)
        {
            var parameter = _parameterFactory.Create<T>(name, value, dbType, direction);
            _parameters.Add(parameter);
            return parameter;
        }

        public void SetParameterValue<T>(string name, T value)
        {
            var parameter = _parameters.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
            if (parameter == null) throw new ApplicationException("Parameter not found " + name);
            parameter.Value = value;
        }

        public IEnumerable<IParameter> GetParameters()
        {
            return _parameters;
        }

        public void Lock()
        {
            Monitor.Enter(_parameters);
        }

        public void Unlock()
        {
            Monitor.Exit(_parameters);
        }
    }
}
