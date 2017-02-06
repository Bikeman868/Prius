using System;
using System.Collections.Generic;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;

namespace Prius.SqLite.Procedures
{
    public class ParameterAccessor : IParameterAccessor
    {
        public T As<T>(IList<IParameter> parameters, string name, T defaultValue = default(T))
        {
            throw new NotImplementedException();
        }

        public IList<IParameter> Sorted(IList<IParameter> parameters, params string[] names)
        {
            throw new NotImplementedException();
        }

        public void Set<T>(IList<IParameter> parameters, string name, T value)
        {
            throw new NotImplementedException();
        }

        public IParameter Find(IList<IParameter> parameters, string name)
        {
            throw new NotImplementedException();
        }

        public void Return<T>(IList<IParameter> parameters, T value)
        {
            throw new NotImplementedException();
        }
    }
}
