using Prius.Contracts.Interfaces;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;
using Prius.SqLite.QueryBuilder;
using Prius.SqLite.StoredProcedures;

namespace UsersTestApp.Procedures
{
    [StoredProcedure("sp_DeleteUser", true)]
    [Parameter("UserID", typeof(long))]
    public class DeleteUser : IAdoProcedure
    {
        private readonly IAdoQueryRunner _queryRunner;

        private IQuery _deleteSql;

        public DeleteUser(IAdoQueryRunner queryRunner, IQueryBuilder queryBuilder)
        {
            _queryRunner = queryRunner;

            _deleteSql = queryBuilder
                .DeleteFrom("tb_Users")
                .Where("UserID = @UserID");
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            _queryRunner.ExecuteNonQuery(context, _deleteSql);
            return null;
        }
    }
}
