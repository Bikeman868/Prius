using System.Linq;
using Prius.Contracts.Attributes;

namespace Prius.Contracts.Interfaces
{
    internal enum Enum1 { Value1, Value2, Value3 }

    /// <summary>
    /// An example of a data contract with declarative mapping to sql fields
    /// </summary>
    internal class Contract1
    {
        [Mapping("name")]
        public string Name { get; set; }

        [Mapping("value", -1)]
        public int Value { get; set; }

        [Mapping("descr", "")]
        public string Description { get; set; }
    }

    /// <summary>
    /// Example of a data contract with mappings defined in code
    /// </summary>
    internal class Contract2: IDataContract<Contract2>
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Enum1 MyEnum { get; set; }

        public void AddMappings(ITypeDefinition<Contract2> typeDefinition, string dataSetName)
        {
            typeDefinition.AddField<string>("name", c => c.Name, string.Empty);
            typeDefinition.AddField<int>("value", c => c.Value, -1);
            typeDefinition.AddField<string>("descr", (c, v) => c.Description = v.ToLower(), string.Empty);
            typeDefinition.AddField<Enum1>("enum", c => c.MyEnum, Enum1.Value1);
        }

        public void SetCalculated(IDataReader dataReader, string dataSetName)
        {
            Title = Name + "=" + Value;
        }
    }

    internal class Profile
    {
        long ProfileId { get; set; }
        string UserName { get; set; }
    }

    internal class MyPage
    {
        private readonly IContextFactory _sqlContextFactory;
        private readonly ICommandFactory _sqlCommandFactory;
        private readonly IMapper _sqlMapper;

        private static ICommand _command;
        private static IParameter _profileIdParameter;

        private long _profileId;

        public MyPage(IContextFactory sqlContextFactory, ICommandFactory sqlCommandFactory, IMapper sqlMapper)
        {
            _sqlContextFactory = sqlContextFactory;
            _sqlCommandFactory = sqlCommandFactory;
            _sqlMapper = sqlMapper;

            if (_command == null)
            {
                _command = _sqlCommandFactory.CreateStoredProcedure("getProfileById");
                _profileIdParameter = _command.AddParameter("id", System.Data.SqlDbType.BigInt);
            }
        }

        public MyPage Initialize(long profileId)
        {
            _profileId = profileId;
            return this;
        }

        public Profile GetProfile_Version1()
        {
            using (var context = _sqlContextFactory.Create("MyPage"))
            {
                _command.Lock();
                try
                {
                    _profileIdParameter.Value = _profileId;
                    context.PrepareCommand(_command);
                }
                finally
                {
                    _command.Unlock();
                }
                var asyncResult = context.BeginExecuteEnumerable();
                return context.EndExecuteEnumerable<Profile>(asyncResult).FirstOrDefault();
            }
        }

        public Profile GetProfile_Version2()
        {
            using (var command = _sqlCommandFactory.CreateStoredProcedure("getProfileById"))
            {
                command.AddParameter<long>("id", _profileId);
                using (var context = _sqlContextFactory.Create("MyPage"))
                {
                    return context.ExecuteEnumerable<Profile>(command).FirstOrDefault();
                }
            }
        }

        public Profile GetProfile_Version3()
        {
            var profile = new Profile();
            using (var command = _sqlCommandFactory.CreateStoredProcedure("getProfileById"))
            {
                command.AddParameter<long>("id", _profileId);
                using (var context = _sqlContextFactory.Create("MyPage"))
                {
                    using (var reader = context.ExecuteReader(command))
                    {
                        if (reader.Read())
                        {
                            var mappedReader = _sqlMapper.GetMappedDataReader<Profile>(reader);
                            mappedReader.Fill(profile);
                        }
                    }
                }
            }
            return profile;
        }
    }

}
