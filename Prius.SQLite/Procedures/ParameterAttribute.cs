using System;
using Prius.Contracts.Attributes;

namespace Prius.SQLite.Procedures
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ParameterAttribute : Attribute
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
        public ParameterAttribute(
            string parameterName,
            Type dataType,
            ParameterDirection direction,
            bool isRequired)
        {
            ParameterName = parameterName;
            DataType = dataType;
            Direction = direction;
            IsRequired = isRequired;

            if (parameterName.StartsWith("@"))
                ParameterName = parameterName.Substring(1);
        }

        /// <summary>
        /// Constructs an attribute that specifies a parameter to a stored procedure
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dataType">The type of data the stored procedure is expecting</param>
        /// <param name="direction">Whether data is passed in, out, or both</param>
        public ParameterAttribute(
            string parameterName,
            Type dataType,
            ParameterDirection direction)
        {
            ParameterName = parameterName;
            DataType = dataType;
            Direction = direction;
            IsRequired = true;

            if (parameterName.StartsWith("@"))
                ParameterName = parameterName.Substring(1);
        }

        /// <summary>
        /// Constructs an attribute that specifies a parameter to a stored procedure
        /// </summary>
        /// <param name="parameterName">The name of the parameter</param>
        /// <param name="dataType">The type of data the stored procedure is expecting</param>
        public ParameterAttribute(
            string parameterName,
            Type dataType)
        {
            ParameterName = parameterName;
            DataType = dataType;
            Direction = ParameterDirection.Input;
            IsRequired = true;

            if (parameterName.StartsWith("@"))
                ParameterName = parameterName.Substring(1);
        }
    }
}
