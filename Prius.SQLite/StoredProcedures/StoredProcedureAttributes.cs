using System;

namespace Prius.SqLite.StoredProcedures
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StoredProcedureAttribute : Attribute
    {
        public string ProcedureName { get; private set; }
        public bool IsThreadSafe { get; private set; }

        /// <summary>
        /// Constructs an attribute that specifies that this class is a stored procedure
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure</param>
        /// <param name="isThreadSafe">Indicates wheher the same instance of this class 
        /// can be used for multiple requests executing in different threads or whether
        /// new instances should be constructed to handle each request</param>
        public StoredProcedureAttribute(string procedureName, bool isThreadSafe)
        {
            ProcedureName = procedureName;
            IsThreadSafe = isThreadSafe;
        }

        /// <summary>
        /// Constructs an attribute that specifies that this class is a stored procedure
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure</param>
        public StoredProcedureAttribute(string procedureName)
        {
            ProcedureName = procedureName;
            IsThreadSafe = false;
        }
    }
}
