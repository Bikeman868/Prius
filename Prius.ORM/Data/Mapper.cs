using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.Orm.Utility;

namespace Prius.Orm.Data
{
    /// <summary>
    /// Provides flexible and extremely efficient mapping between data readers and data contracts
    /// </summary>
    /// <remarks>
    /// This code assumes that:
    /// 1. There are classes that are used as containers for data retrieved from the database (data contracts) that
    ///    can be mapped onto domain objects. In some cases the domain objects themselves can also be the data contract to avoid an extra
    ///    mapping step. 
    /// 2. There are a number of different ways to return data from the database and populate the same data contract. These different ways should
    ///    be assigned data set names by the developer. If every stored procedure returns a different shape of data, then the name of the
    ///    stored procedure can be used as the data set name.
    /// 3. If two stored procedures return the same data with the same column name and same data tye then they can use the same data set name.
    ///    These stored procedures can return different sub-sets of columns and the columns can be in a different order, and this is still considered
    ///    to be the same data set name.
    /// 4. Different data set names are required if two stored procedures return the same information with a different column name or with a 
    ///    different data type.
    /// To use the mapper:
    /// 1. Either decorate your data contract classes' properties with the MappingAttribute, or implement the IDataContract interface, or both.
    /// 2. Execute a stored procedure and obtain an IDataReader instance.
    /// 3. Call the Map() method of the mapper to create a new data contract populated with the current record in the data reader.
    /// 4. Or call the Fill() method of the mapper to fill a data contract that you constructed before hand.
    /// 5. Or call the GetMappedDataReader() method to build a column mapper that can Fill or Map data contracts more efficiently for this data reader.
    /// </remarks>
    public class Mapper: IMapper
    {
        private readonly IFactory _defaultFactory;
        private readonly IErrorReporter _errorReporter;

        private IThreadSafeDictionary<string, TypeDefinition> _typeDefinitions;

        public Mapper(IFactory defaultFactory, IErrorReporter errorReporter)
        {
            _defaultFactory = defaultFactory;
            _errorReporter = errorReporter;
        }

        public IMapper Initialize()
        {
            _typeDefinitions = new ThreadSafeDictionary<string, TypeDefinition>();
            return this;
        }

        private TypeDefinition<T> GetTypeDefinition<T>(string dataSetName, IFactory<T> dataContractFactory) where T : class
        {
            var key = typeof(T).FullName;
            if (dataSetName != null) key += ":" + dataSetName;

            return (TypeDefinition<T>)_typeDefinitions.GetOrAdd(key, k => CreateTypeDefinition(dataSetName, dataContractFactory));
        }

        private TypeDefinition<T> CreateTypeDefinition<T>(string dataSetName, IFactory<T> dataContractFactory) where T : class
        {
            var typedTypeDefinition = new TypeDefinition<T>(_defaultFactory, _errorReporter).Initialize(dataSetName, dataContractFactory);

            // Add mappings for fields that have [Mapping] attributes attached
            foreach (var property in typeof(T).GetProperties())
            {
                foreach (var mappingAttribute in property
                    .GetCustomAttributes(true)
                    .Select(a => a as MappingAttribute)
                    .Where(a => a != null))
                {
                    if (mappingAttribute.DefaultValue != null && !property.PropertyType.IsAssignableFrom(mappingAttribute.DefaultValue.GetType()))
                        throw new Exception("The default value in the mapping attribute must match the type of property the mapping references. Field: " + mappingAttribute.FieldName + ", Type: " + typeof(T).FullName);

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
                    if (disposable != null)
                        disposable.Dispose();
                }
            }

            return typedTypeDefinition;
        }

        public TDataContract Map<TDataContract>(IDataReader dataReader, string dataSetName = null, IFactory<TDataContract> dataContractFactory = null) 
            where TDataContract : class
        {
            var typeDefinition = GetTypeDefinition<TDataContract>(dataSetName, dataContractFactory);
            var dataContract = _defaultFactory.Create<TDataContract>();
            typeDefinition.Fill(dataContract, dataReader);
            return dataContract;
        }

        public void Fill<TDataContract>(TDataContract dataContract, IDataReader dataReader, string dataSetName)
            where TDataContract : class
        {
            var typeDefinition = GetTypeDefinition<TDataContract>(dataSetName, null);
            typeDefinition.Fill(dataContract, dataReader);
        }

        public IMappedDataReader<TDataContract> GetMappedDataReader<TDataContract>(IDataReader dataReader, string dataSetName, IFactory<TDataContract> dataContractFactory) 
            where TDataContract : class
        {
            var typeDefinition = GetTypeDefinition<TDataContract>(dataSetName, dataContractFactory);
            return typeDefinition.GetMappedDataReader(dataReader);
        }

        private abstract class TypeDefinition
        {
        }

        /// <summary>
        /// Describes how to map database columns onto properties of a particular data contract type for a specific data set.
        /// </summary>
        private class TypeDefinition<TDataContract> : TypeDefinition, IFactory<TDataContract>, ITypeDefinition<TDataContract> where TDataContract: class
        {
            private readonly IFactory _dependencyResolver;
            private readonly IErrorReporter _errorReporter;

            private IDictionary<string, FieldDefinition<TDataContract>> _fieldDefinitions;
            private IDictionary<string, DataReaderMapping<TDataContract>> _dataReaderMappings;
            private IFactory<TDataContract> _dataContractFactory;
            private string _dataSetName;

            public TypeDefinition(
                IFactory dependencyResolver,
                IErrorReporter errorReporter)
            {
                _dependencyResolver = dependencyResolver;
                _errorReporter = errorReporter;
            }

            public TypeDefinition<TDataContract> Initialize(string dataSetName, IFactory<TDataContract> dataContractFactory)
            {
                _fieldDefinitions = new ThreadSafeDictionary<string, FieldDefinition<TDataContract>>();
                _dataReaderMappings = new ThreadSafeDictionary<string, DataReaderMapping<TDataContract>>();
                _dataContractFactory = dataContractFactory;
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

                    else throw new Exception("Mapping with type of: " + type.FullName + " are not currently supported. Please add to Mapper.cs");
                }
                else if (type.IsNullable())
                {
                    AddField<Object>(fieldName, property, defaultValue);
                }
                else if (type.IsEnum)
                {
                    throw new ApplicationException("You can not add Mapping attributes to properties that are Enums. Please implement IDataContract<T> in your data contract for these properties. " + type.FullName + " " + property.Name);
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
                        try
                        {
                            field.WriteAction((TDataContract)dataContract, dataReader.Get(fieldIndex, field.DefaultValue, field.PropertyType));
                        }
                        catch (Exception ex)
                        {
                            _errorReporter.ReportError(ex, "Error filling " + typeof(TDataContract).FullName + " from database field " + fieldName);
                        }
                    }
                    else
                    {
                        _errorReporter.ReportError(null, "Prius Mapper. Error filling " + typeof(TDataContract).FullName + " there is no mapping for field " + fieldName);
                    }
                }
                var dataContractInterface = dataContract as IDataContract<TDataContract>;
                if (dataContractInterface != null) dataContractInterface.SetCalculated(dataReader, _dataSetName);
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
                return new MappedDataReader<TDataContract>(this, _errorReporter).Initialize(mapping, dataReader);
            }

            public TDataContract Create()
            {
                if (_dataContractFactory != null) return _dataContractFactory.Create();
                return _dependencyResolver.Create<TDataContract>();
            }
        }

        /// <summary>
        /// Defines a mapping between the columns of a data reader and the properties of a data contract.
        /// This mapping can be figured once and reused for maximum performance.
        /// </summary>
        private class DataReaderMapping<TDataContract> where TDataContract: class
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
            private readonly IErrorReporter _errorReporter;

            private DataReaderMapping<TDataContract> _mapping;
            private IDataReader _dataReader;

            public MappedDataReader(IFactory<TDataContract> dataContractFactory, IErrorReporter errorReporter)
            {
                _dataContractFactory = dataContractFactory;
                _errorReporter = errorReporter;
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
                    if (field != null)
                    {
                        object value = null;
                        try
                        {
                            value = _dataReader.Get(fieldIndex, field.DefaultValue, field.PropertyType);
                        }
                        catch (Exception ex)
                        {
                            var msg = string.Format("Exception getting field #{0} of type {1} with default value {2}",
                                fieldIndex, 
                                field.PropertyType.Name, 
                                field.DefaultValue);
                            throw new Exception(msg, ex);
                        }
                        try
                        {
                            field.WriteAction(dataContract, value);
                        }
                        catch (Exception ex)
                        {
                            var msg = string.Format("Exception writing field #{0} of type {1} with default value {2} from value '{3}' of type {4} into {5} data contract",
                                fieldIndex, 
                                field.PropertyType.Name, 
                                field.DefaultValue, 
                                value, value == null ? "<null>" : value.GetType().Name,
                                dataContract.GetType().Name);
                            _errorReporter.ReportError(ex, msg);
                        }
                    }
                }
                var dataContractIntraface = dataContract as IDataContract<TDataContract>;
                if (dataContractIntraface != null) dataContractIntraface.SetCalculated(_dataReader, _mapping.DataSetName);
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
        private class FieldDefinition<TDataContract> where TDataContract: class
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
