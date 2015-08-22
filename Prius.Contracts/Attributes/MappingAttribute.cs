using System;

namespace Prius.Contracts.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public class MappingAttribute : Attribute
    {
        public string FieldName { get; private set; }
        public object DefaultValue { get; private set; }

        public MappingAttribute(string fieldName, object defaultValue)
        {
            FieldName = fieldName;
            DefaultValue = defaultValue;
        }

        public MappingAttribute(string fieldName)
        {
            FieldName = fieldName;
            DefaultValue = null;
        }
    }

}
