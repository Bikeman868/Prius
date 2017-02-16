using Prius.Contracts.Interfaces;
using Prius.SQLite.Interfaces;
using Prius.SQLite.Procedures;
using Prius.SQLite.QueryBuilder;

namespace UsersTestApp.Procedures
{
    [Procedure("sp_GetUsers")]
    public class GetUsers : IAdoProcedure
    {
        private readonly IAdoQueryRunner _queryRunner;
        private readonly IDataReaderFactory _dataReaderFactory;

        private readonly IQuery _selectSql;

        public GetUsers(
            IAdoQueryRunner queryRunner, 
            IDataReaderFactory dataReaderFactory,
            IQueryBuilder queryBuilder)
        {
            _queryRunner = queryRunner;
            _dataReaderFactory = dataReaderFactory;

            _selectSql = queryBuilder
                .Select("UserID", "FirstName", "LastName", "DateOfBirth")
                .From("tb_Users");
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            var sqLiteReader = _queryRunner.ExecuteReader(context, _selectSql);
            return _dataReaderFactory.Create(sqLiteReader, context);
        }
    }
}
