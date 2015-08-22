using System;
using System.Linq.Expressions;

namespace Prius.Contracts.Interfaces
{
    public interface ITypeDefinition<TDataContract> where TDataContract : class
    {
        void AddField<TProperty>(string fieldName, Expression<Func<TDataContract, TProperty>> property, TProperty defaultValue);
        void AddField<TProperty>(string fieldName, Action<TDataContract, TProperty> writeAction, TProperty defaultValue);
    }
}
