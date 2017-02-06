using System;
using System.Collections.Generic;
using System.Linq;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Utility;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.Procedures
{
    public class ParameterAccessor : IParameterAccessor
    {
        public T As<T>(IList<IParameter> parameters, string name, T defaultValue = default(T))
        {
            var parameter = Find(parameters, name);
            if (parameter == null) return defaultValue;

            var resultType = typeof(T);
            if (resultType.IsNullable())
            {
                if (parameter.Value == null)
                    return defaultValue;
                resultType = resultType.GetGenericArguments()[0];
            }
            return (T)Convert.ChangeType(parameter.Value, resultType);
        }

        public IList<IParameter> Sorted(IList<IParameter> parameters, params string[] names)
        {
            return names.Select(name => Find(parameters, name)).ToList();
        }

        public void Set<T>(IList<IParameter> parameters, string name, T value)
        {
            var parameter = Find(parameters, name);
            if (parameter != null)
            {
                parameter.Value = value;
            }
        }

        public IParameter Find(IList<IParameter> parameters, string name)
        {
            if (parameters == null || parameters.Count == 0 || string.IsNullOrEmpty(name)) 
                return null;

            if (name[0] != '@') name = "@" + name;
            return parameters.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public void Return<T>(IList<IParameter> parameters, T value)
        {
            var returnParam = parameters.FirstOrDefault(p => p.Direction == ParameterDirection.ReturnValue);
            if (returnParam != null)
                returnParam.Value = value;
        }
    }
}
