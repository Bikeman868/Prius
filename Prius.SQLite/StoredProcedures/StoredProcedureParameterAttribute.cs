using System;
using Prius.Contracts.Attributes;

namespace Prius.SqLite.StoredProcedures
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StoredProcedureParameterAttribute : Attribute
    {
        public string ParameterName { get; private set; }
        public Type DataType { get; private set; }
        public ParameterDirection Direction { get; private set; }
        public bool IsRequired { get; private set; }

        /// <summary>
        /// Constructs an attribute that specifies a parameter to a stored procedure
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dataType">The type of data the stored procedure is expecting</param>
        /// <param name="direction">Whether data is passed in, out, or both</param>
        /// <param name="isRequired">Can this parameter be omitted (null)</param>
        public StoredProcedureParameterAttribute(
            string parameterName,
            Type dataType,
            ParameterDirection direction,
            bool isRequired)
        {
            ParameterName = parameterName;
            DataType = dataType;
            Direction = direction;
            IsRequired = isRequired;
        }

        /// <summary>
        /// Constructs an attribute that specifies a parameter to a stored procedure
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dataType">The type of data the stored procedure is expecting</param>
        public StoredProcedureParameterAttribute(
            string parameterName,
            Type dataType)
        {
            ParameterName = parameterName;
            DataType = dataType;
            Direction = ParameterDirection.Input;
            IsRequired = true;
        }
    }
}
