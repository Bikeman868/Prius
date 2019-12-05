using System;
using System.Collections.Generic;
using System.Linq;
using Moq.Modules;
using Prius.Contracts.Exceptions;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Attributes;
using System.Reflection;
using System.Linq.Expressions;
using Prius.Contracts.Interfaces.Commands;
using Prius.Contracts.Interfaces.External;

namespace Prius.Mocks
{
    public class MockMapper: ConcreteImplementationProvider<IMapper>
    {
        protected override IMapper GetImplementation(IMockProducer mockProducer)
        {
            return new Mapper().Initialize();
        }

        private class Mapper : IMapper
        {
            private IDictionary<string, TypeDefinition> _typeDefinitions;

            public Mapper()
            {
            }

            public IMapper Initialize()
            {
                _typeDefinitions = new Dictionary<string, TypeDefinition>();
                return this;
            }

            private TypeDefinition<T> GetTypeDefinition<T>(string dataSetName) where T : class
            {
                var key = typeof(T).FullName;
                if (dataSetName != null) key += ":" + dataSetName;

                if (_typeDefinitions.ContainsKey(key))
                    return (TypeDefinition<T>)(_typeDefinitions[key]);

                var typeDefinition = CreateTypeDefinition<T>(dataSetName);
                _typeDefinitions[key] = typeDefinition;

                return (TypeDefinition<T>)typeDefinition;
            }

            private TypeDefinition<T> CreateTypeDefinition<T>(string dataSetName) where T : class
            {
                var typedTypeDefinition = new TypeDefinition<T>().Initialize(dataSetName);

                // Add mappings for fields that have [Mapping] attributes attached
                foreach (var property in typeof(T).GetProperties())
                {
                    foreach (var mappingAttribute in property
                        .GetCustomAttributes(true)
                        .Select(a => a as MappingAttribute)
                        .Where(a => a != null))
                    {
                        if (mappingAttribute.DefaultValue != null && !property.PropertyType.IsInstanceOfType(mappingAttribute.DefaultValue))
                            throw new PriusException("The default value in the mapping attribute must match the type of property the mapping references. Field: " + mappingAttribute.FieldName + ", Type: " + typeof(T).FullName);

                        typedTypeDefinition.AddField(mappingAttribute.FieldName, property, mappingAttribute.DefaultValue);
                    }
                }

                // Apply any custom mapping behaviour
                if (typeof(IDataContract<T>).IsAssignableFrom(typeof(T)))
                {
                    var dataContract = typedTypeDefinition.Create() as IDataContract<T>;
                    try
                    {
                        dataContract.AddMappings(typedTypeDefinition, dataSetName);
                    }
                    finally
                    {
                        var disposable = dataContract as IDisposable;
                        disposable?.Dispose();
                    }
                }

                return typedTypeDefinition;
            }

            public TDataContract Map<TDataContract>(IDataReader dataReader, string dataSetName = null, IFactory<TDataContract> dataContractFactory = null)
                where TDataContract : class
            {
                var typeDefinition = GetTypeDefinition<TDataContract>(dataSetName);
                var dataContract = (TDataContract)(typeof(TDataContract).GetConstructor(Type.EmptyTypes).Invoke(null));
                typeDefinition.Fill(dataContract, dataReader);
                return dataContract;
            }

            public void Fill<TDataContract>(TDataContract dataContract, IDataReader dataReader, string dataSetName)
                where TDataContract : class
            {
                var typeDefinition = GetTypeDefinition<TDataContract>(dataSetName);
                typeDefinition.Fill(dataContract, dataReader);
            }

            public IMappedDataReader<TDataContract> GetMappedDataReader<TDataContract>(IDataReader dataReader, string dataSetName, IFactory<TDataContract> dataContractFactory)
                where TDataContract : class
            {
                var typeDefinition = GetTypeDefinition<TDataContract>(dataSetName);
                return typeDefinition.GetMappedDataReader(dataReader);
            }

            private abstract class TypeDefinition
            {
            }

            /// <summary>
            /// Describes how to map database columns onto properties of a particular data contract type for a specific data set.
            /// </summary>
            private class TypeDefinition<TDataContract> : TypeDefinition, IFactory<TDataContract>, ITypeDefinition<TDataContract> where TDataContract : class
            {
                private IDictionary<string, FieldDefinition<TDataContract>> _fieldDefinitions;
                private IDictionary<string, DataReaderMapping<TDataContract>> _dataReaderMappings;
                private string _dataSetName;

                public TypeDefinition<TDataContract> Initialize(string dataSetName)
                {
                    _fieldDefinitions = new Dictionary<string, FieldDefinition<TDataContract>>();
                    _dataReaderMappings = new Dictionary<string, DataReaderMapping<TDataContract>>();
                    _dataSetName = dataSetName;
                    return this;
                }

                public void AddField(string fieldName, PropertyInfo property, object defaultValue)
                {
                    var type = property.PropertyType;

                    if (type == typeof(string))
                    {
                        AddField<String>(fieldName, property, defaultValue);
                    }
                    else if (type.BaseType == typeof(ValueType))
                    {
                        if (type == typeof(Int16)) AddField<Int16>(fieldName, property, defaultValue);
                        else if (type == typeof(Int32)) AddField<Int32>(fieldName, property, defaultValue);
                        else if (type == typeof(Int64)) AddField<Int64>(fieldName, property, defaultValue);
                        else if (type == typeof(Single)) AddField<Single>(fieldName, property, defaultValue);
                        else if (type == typeof(Double)) AddField<Double>(fieldName, property, defaultValue);
                        else if (type == typeof(DateTime)) AddField<DateTime>(fieldName, property, defaultValue);
                        else if (type == typeof(Boolean)) AddField<Boolean>(fieldName, property, defaultValue);
                        else if (type == typeof(Decimal)) AddField<Decimal>(fieldName, property, defaultValue);
                        else if (type == typeof(Guid)) AddField<Guid>(fieldName, property, defaultValue);

                        else if (type == typeof(Int16?)) AddField<Int16?>(fieldName, property, defaultValue);
                        else if (type == typeof(Int32?)) AddField<Int32?>(fieldName, property, defaultValue);
                        else if (type == typeof(Int64?)) AddField<Int64?>(fieldName, property, defaultValue);
                        else if (type == typeof(Single?)) AddField<Single?>(fieldName, property, defaultValue);
                        else if (type == typeof(Double?)) AddField<Double?>(fieldName, property, defaultValue);
                        else if (type == typeof(DateTime?)) AddField<DateTime?>(fieldName, property, defaultValue);
                        else if (type == typeof(Boolean?)) AddField<Boolean?>(fieldName, property, defaultValue);
                        else if (type == typeof(Decimal?)) AddField<Decimal?>(fieldName, property, defaultValue);
                        else if (type == typeof(Guid?)) AddField<Guid?>(fieldName, property, defaultValue);

                        else throw new PriusException("Mapping with type of: " + type.FullName + " are not currently supported. Please add to Mapper.cs");
                    }
                    else if (type.IsGenericType && type.BaseType == typeof(ValueType))
                    {
                        AddField<Object>(fieldName, property, defaultValue);
                    }
                    else if (type.IsEnum)
                    {
                        throw new PriusException("You can not add Mapping attributes to properties that are Enums. Please implement IDataContract<T> in your data contract for these properties. " + type.FullName + " " + property.Name);
                        //AddField<Int32>(fieldName, (TDataContract dc, Int32 v) => property.SetValue(dc, Enum.ToObject(type, v), null), 0);
                    }
                    else AddField<Object>(fieldName, property, defaultValue);
                }

                public void AddField<TProperty>(string fieldName, Expression<Func<TDataContract, TProperty>> property, TProperty defaultValue)
                {
                    var body = property.Body as MemberExpression;
                    if (body == null) return;

                    var propertyInfo = body.Member as PropertyInfo;
                    if (propertyInfo == null) return;

                    AddField(fieldName, propertyInfo, defaultValue);
                }

                public void AddField<T>(string fieldName, PropertyInfo property, object defaultValue)
                {
                    if (defaultValue == null) defaultValue = default(T);
                    AddField<T>(fieldName, property, (T)defaultValue);
                }

                public void AddField<TProperty>(string fieldName, object writeAction, TProperty defaultValue)
                {
                    AddField<TProperty>(fieldName, (Action<TDataContract, TProperty>)writeAction, defaultValue);
                }

                public void AddField<TProperty>(string fieldName, Action<TDataContract, TProperty> writeAction, TProperty defaultValue)
                {
                    var field = new FieldDefinition<TDataContract, TProperty>
                    {
                        PropertyType = typeof(TProperty),
                        WriteAction = writeAction,
                        DefaultValue = defaultValue
                    };
                    _fieldDefinitions[fieldName.ToLower()] = field;
                }

                public void AddField<TProperty>(string fieldName, PropertyInfo property, TProperty defaultValue)
                {
                    var targetExpression = Expression.Parameter(typeof(TDataContract), "t");
                    var valueExpression = Expression.Parameter(typeof(TProperty), "v");
                    var propertyExpression = Expression.Property(targetExpression, property);
                    var assignmentExpression = Expression.Assign(propertyExpression, valueExpression);
                    var writeAction = Expression.Lambda<Action<TDataContract, TProperty>>(assignmentExpression, targetExpression, valueExpression).Compile();

                    var field = new FieldDefinition<TDataContract, TProperty>
                    {
                        DefaultValue = defaultValue,
                        PropertyType = property.PropertyType,
                        WriteAction = writeAction
                    };
                    _fieldDefinitions[fieldName.ToLower()] = field;
                }

                public void Fill(object dataContract, IDataReader dataReader)
                {
                    for (var fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                    {
                        var fieldName = dataReader.GetFieldName(fieldIndex).ToLower();
                        FieldDefinition<TDataContract> field;
                        if (_fieldDefinitions.TryGetValue(fieldName, out field))
                        {
                            field.WriteAction((TDataContract)dataContract, dataReader.Get(fieldIndex, field.DefaultValue, field.PropertyType));
                        }
                        else
                        {
                            throw new PriusException("Error filling " + typeof(TDataContract).FullName + " there is no mapping for field " + fieldName);
                        }
                    }
                    var dataContractInterface = dataContract as IDataContract<TDataContract>;
                    dataContractInterface?.SetCalculated(dataReader, _dataSetName);
                }

                private DataReaderMapping<TDataContract> GetDataReaderMapping(IDataReader dataReader)
                {
                    DataReaderMapping<TDataContract> result;
                    if (_dataReaderMappings.TryGetValue(dataReader.DataShapeName, out result)) return result;
                    lock (_dataReaderMappings)
                    {
                        if (_dataReaderMappings.TryGetValue(dataReader.DataShapeName, out result)) return result;
                        result = new DataReaderMapping<TDataContract>(dataReader, _fieldDefinitions, _dataSetName);
                        _dataReaderMappings.Add(dataReader.DataShapeName, result);
                        return result;
                    }
                }

                public IMappedDataReader<TDataContract> GetMappedDataReader(IDataReader dataReader)
                {
                    var mapping = GetDataReaderMapping(dataReader);
                    return new MappedDataReader<TDataContract>(this).Initialize(mapping, dataReader);
                }

                public TDataContract Create()
                {
                    return (TDataContract)(typeof(TDataContract).GetConstructor(Type.EmptyTypes).Invoke(null));
                }
            }

            /// <summary>
            /// Defines a mapping between the columns of a data reader and the properties of a data contract.
            /// This mapping can be figured once and reused for maximum performance.
            /// </summary>
            private class DataReaderMapping<TDataContract> where TDataContract : class
            {
                public IList<FieldDefinition<TDataContract>> Fields { get; private set; }
                public string DataSetName { get; private set; }

                public DataReaderMapping(IDataReader dataReader, IDictionary<string, FieldDefinition<TDataContract>> fieldDefinitions, string dataSetName)
                {
                    DataSetName = dataSetName;
                    Fields = new List<FieldDefinition<TDataContract>>();
                    for (var fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                    {
                        var fieldName = dataReader.GetFieldName(fieldIndex).ToLower();
                        FieldDefinition<TDataContract> field;
                        if (fieldDefinitions.TryGetValue(fieldName, out field))
                            Fields.Add(field);
                        else
                            Fields.Add(null);
                    }
                }
            }

            /// <summary>
            /// Provides a way to quickly populate data contracts from the current record in a data reader given
            /// information about the properties that are mapped to each column ordinal
            /// </summary>
            private class MappedDataReader<TDataContract> : IMappedDataReader<TDataContract> where TDataContract : class
            {
                private readonly IFactory<TDataContract> _dataContractFactory;
                private DataReaderMapping<TDataContract> _mapping;
                private IDataReader _dataReader;

                public MappedDataReader(IFactory<TDataContract> dataContractFactory)
                {
                    _dataContractFactory = dataContractFactory;
                }

                public IMappedDataReader<TDataContract> Initialize(DataReaderMapping<TDataContract> mapping, IDataReader dataReader)
                {
                    _mapping = mapping;
                    _dataReader = dataReader;
                    return this;
                }

                public TDataContract Fill(TDataContract dataContract)
                {
                    if (dataContract == null) dataContract = _dataContractFactory.Create();

                    for (var fieldIndex = 0; fieldIndex < _mapping.Fields.Count; fieldIndex++)
                    {
                        var field = _mapping.Fields[fieldIndex];
                        field?.WriteAction((TDataContract)dataContract, _dataReader.Get(fieldIndex, field.DefaultValue, field.PropertyType));
                    }
                    var dataContractIntraface = dataContract as IDataContract<TDataContract>;
                    dataContractIntraface?.SetCalculated(_dataReader, _mapping.DataSetName);
                    return dataContract;
                }

                public TDataContract Map()
                {
                    return Fill(null);
                }

                public string DataShapeName
                {
                    get { return _dataReader.DataShapeName; }
                }

                public bool IsServerOffline { get { return _dataReader.IsServerOffline; } }

                public Exception ServerOfflineException { get { return _dataReader.ServerOfflineException; } }

                public int FieldCount
                {
                    get { return _dataReader.FieldCount; }
                }

                public object this[int fieldIndex]
                {
                    get { return _dataReader[fieldIndex]; }
                }

                public object this[string fieldName]
                {
                    get { return _dataReader[fieldName]; }
                }

                public string GetFieldName(int fieldIndex)
                {
                    return _dataReader.GetFieldName(fieldIndex);
                }

                public int GetFieldIndex(string fieldName)
                {
                    return _dataReader.GetFieldIndex(fieldName);
                }

                public bool IsNull(int fieldIndex)
                {
                    return _dataReader.IsNull(fieldIndex);
                }

                public bool Read()
                {
                    return _dataReader.Read();
                }

                public bool NextResult()
                {
                    return _dataReader.NextResult();
                }

                public T Get<T>(int fieldIndex, T defaultValue)
                {
                    return _dataReader.Get<T>(fieldIndex, defaultValue);
                }

                public T Get<T>(string fieldName, T defaultValue = default(T))
                {
                    return _dataReader.Get<T>(fieldName, defaultValue);
                }

                public object Get(int fieldIndex, object defaultValue, Type type)
                {
                    return _dataReader.Get(fieldIndex, defaultValue, type);
                }

                public bool IsReusable
                {
                    get { return false; }
                }

                public bool IsDisposing
                {
                    get { return false; }
                }

                public bool IsDisposed
                {
                    get { return false; }
                }

                public void Dispose()
                {
                }
            }

            /// <summary>
            /// Base class for FieldDefinition<TDataContract, TProperty> that allows them to be contained in a generic collection where TProperty varies
            /// </summary>
            private class FieldDefinition<TDataContract> where TDataContract : class
            {
                public Type PropertyType;
                public object DefaultValue;
                public Action<TDataContract, object> WriteAction;
            }

            /// <summary>
            /// Defines how to populate the data contract with information from a specific field in the database results
            /// </summary>
            private class FieldDefinition<TDataContract, TProperty> : FieldDefinition<TDataContract> where TDataContract : class
            {
                public new TProperty DefaultValue { set { base.DefaultValue = value; } }
                public new Action<TDataContract, TProperty> WriteAction { set { base.WriteAction = (c, v) => value(c, (TProperty)v); } }
            }

        }
    }
}
