using Prius.Contracts.Interfaces;
using Prius.SQLite.Interfaces;
using Prius.SQLite.Procedures;
using Prius.SQLite.QueryBuilder;

namespace UsersTestApp.Procedures
{
    [Procedure("sp_DeleteUser")]
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
