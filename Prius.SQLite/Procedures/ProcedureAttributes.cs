using System;

namespace Prius.SqLite.Procedures
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProcedureAttribute : Attribute
    {
        public string ProcedureName { get; private set; }
        public string RepositoryName { get; private set; }
        public bool IsThreadSafe { get; private set; }

        /// <summary>
        /// Constructs an attribute that specifies that this class is a stored procedure
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure</param>
        /// <param name="repositoryName">The name of the repository that contains this
        /// procedure or null to make this available in all repositories</param>
        /// <param name="isThreadSafe">Indicates wheher the same instance of this class 
        /// can be used for multiple requests executing in different threads or whether
        /// new instances should be constructed to handle each request</param>
        public ProcedureAttribute(string procedureName, string repositoryName, bool isThreadSafe)
        {
            ProcedureName = procedureName;
            RepositoryName = repositoryName;
            IsThreadSafe = isThreadSafe;
        }

        /// <summary>
        /// Constructs an attribute that specifies that this class is a stored procedure
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure</param>
        /// <param name="repositoryName">The name of the repository that contains this
        /// procedure or null to make this available in all repositories</param>
        public ProcedureAttribute(string procedureName, string repositoryName)
        {
            ProcedureName = procedureName;
            RepositoryName = repositoryName;
            IsThreadSafe = true;
        }

        /// <summary>
        /// Constructs an attribute that specifies that this class is a stored procedure
        /// </summary>
        /// <param name="procedureName">The name of the stored procedure</param>
        public ProcedureAttribute(string procedureName)
        {
            ProcedureName = procedureName;
            RepositoryName = null;
            IsThreadSafe = true;
        }
    }
}
