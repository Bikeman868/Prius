using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;
using Prius.SqLite.StoredProcedures;

namespace UsersTestApp.Procedures
{
    [StoredProcedure("sp_InsertUser", true)]
    [Parameter("FirstName", typeof(string))]
    [Parameter("LastName", typeof(string))]
    [Parameter("UserID", typeof(long), ParameterDirection.ReturnValue, false)]
    public class InsertUser : IAdoProcedure
    {
        private readonly IAdoQueryRunner _queryRunner;
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IParameterAccessor _parameterAccessor;

        public InsertUser(
            IAdoQueryRunner queryRunner, 
            IDataReaderFactory dataReaderFactory, 
            IParameterAccessor parameterAccessor)
        {
            _queryRunner = queryRunner;
            _dataReaderFactory = dataReaderFactory;
            _parameterAccessor = parameterAccessor;
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            _queryRunner.ExecuteNonQuery(context, "INSERT INTO tb_Users (FirstName,LastName) VALUES (@FirstName,@LastName)");
            _parameterAccessor.Set(context.Parameters, "UserID", context.Connection.LastInsertRowId);

            var sqLiteReader = _queryRunner.ExecuteReader(context, "SELECT UserID, FirstName, LastName FROM tb_Users WHERE FirstName = @FirstName AND LastName = @LastName");

            return _dataReaderFactory.Create(sqLiteReader, context);
        }
    }
}
