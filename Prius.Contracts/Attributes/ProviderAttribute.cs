using System;

namespace Prius.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ProviderAttribute : Attribute
    {
        public string ServerType { get; private set; }
        public string Name { get; private set; }

        /// <summary>
        /// Constructs an attribute that marks this class as a provider of connections
        /// to a specific database engine
        /// </summary>
        /// <param name="serverType">The value to put into the application config to 
        /// choose this provider for a database</param>
        /// <param name="name">The display name for this provider</param>
        public ProviderAttribute(string serverType, string name)
        {
            ServerType = serverType;
            Name = name;
        }
    }

}
