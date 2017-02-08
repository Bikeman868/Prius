using System.Collections.Generic;
using Prius.Contracts.Interfaces.Commands;

namespace Prius.SqLite.Interfaces
{
    /// <summary>
    /// Provides some convenient helper methods for dealing with the parameters that are pssed into
    /// stored procedures.
    /// </summary>
    public interface IParameterAccessor
    {
        /// <summary>
        /// Finds a parameter by name and returns its value as a specific type
        /// </summary>
        /// <typeparam name="T">The type of data to return</typeparam>
        /// <param name="parameters">A list of parameters passed (usually to a procedure)</param>
        /// <param name="name">The name of the parameter to find</param>
        /// <param name="defaultValue">The default value to use if this parameter is not in the list</param>
        /// <returns>The value of the specified parameter</returns>
        T As<T>(IList<IParameter> parameters, string name, T defaultValue = default(T));

        /// <summary>
        /// Sets the value of an output parameter if there is one
        /// </summary>
        /// <typeparam name="T">The type of value being uses to set the output parameter</typeparam>
        /// <param name="parameters">A list of parameters passed (usually to a procedure)</param>
        /// <param name="name">The name of the parameter to set</param>
        /// <param name="value">The value to set</param>
        void Set<T>(IList<IParameter> parameters, string name, T value);

        /// <summary>
        /// Finds a parameter by name
        /// </summary>
        /// <param name="parameters">A list of parameters passed (usually to a procedure)</param>
        /// <param name="name">The name of the parameter to find</param>
        /// <returns>A parameter or null if it was not found</returns>
        IParameter Find(IList<IParameter> parameters, string name);

        /// <summary>
        /// Sets the value of the 'return' parameter if there is one
        /// </summary>
        /// <typeparam name="T">The type of value being uses to set the return value</typeparam>
        /// <param name="parameters">A list of parameters passed (usually to a procedure)</param>
        /// <param name="value">The value to return</param>
        void Return<T>(IList<IParameter> parameters, T value);

        /// <summary>
        /// Takes a list of procedure parameters and sorts them into a specific order so that
        /// they can be accessed more efficiently by index.
        /// </summary>
        /// <param name="parameters">A list of procedure parameters</param>
        /// <param name="names">The names of the parameters to return at the front of the list</param>
        /// <returns>The list of parameters in the same order as the names parameter</returns>
        IList<IParameter> Sorted(IList<IParameter> parameters, params string[] names);
    }
}
