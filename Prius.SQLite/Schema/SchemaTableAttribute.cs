using System;

namespace Prius.SqLite.Schema
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SchemaTableAttribute : Attribute
    {
        public string RepositoryName { get; private set; }
        public string TableName { get; private set; }

        /// <summary>
        /// Constructs an attribute that marks this class as a database table schema. You can
        /// attach multiple attributes is this same table scheme is used in multiple repositories
        /// or if there are miltiple tables in th repository with the same schema (unlikely)
        /// </summary>
        /// <param name="tableName">The name of the database table to create</param>
        /// <param name="repositoryName">The Prius repository where this table exists</param>
        public SchemaTableAttribute(string tableName, string repositoryName)
        {
            RepositoryName = repositoryName;
            TableName = tableName;
        }

        /// <summary>
        /// Constructs an attribute that marks this class as a database table that applies to
        /// all schemas. This form of the constructor is designed for the case where you have
        /// only one repository (common) or many identical repositories (for example multi-tennant).
        /// </summary>
        /// <param name="tableName">The name of the database table to create in all databases</param>
        public SchemaTableAttribute(string tableName)
        {
            RepositoryName = null;
            TableName = tableName;
        }
    }
}
