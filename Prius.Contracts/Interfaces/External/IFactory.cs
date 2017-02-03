using System;

namespace Prius.Contracts.Interfaces.External
{
    public interface IFactory
    {
        T Create<T>() where T: class;
        object Create(Type type);
    }

    public interface IFactory<out T> where T : class
    {
        T Create();
    }
}
