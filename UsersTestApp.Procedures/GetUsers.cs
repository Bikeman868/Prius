using Prius.Contracts.Interfaces;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;

namespace UsersTestApp.Procedures
{
    [StoredProcedure("sp_GetUsers", true)]
    public class GetUsers : IAdoProcedure
    {
        private readonly IAdoQueryRunner _queryRunner;
        private readonly IDataReaderFactory _dataReaderFactory;

        public GetUsers(
            IAdoQueryRunner queryRunner, 
            IDataReaderFactory dataReaderFactory)
        {
            _queryRunner = queryRunner;
            _dataReaderFactory = dataReaderFactory;
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            var sqLiteReader = _queryRunner.ExecuteReader(context, "SELECT UserID, FirstName, LastName FROM tb_Users");
            return _dataReaderFactory.Create(sqLiteReader, context);
        }
    }
}
