using System;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.SqLite.Interfaces;
using Prius.SqLite.Procedures;
using Prius.SqLite.QueryBuilder;
using Prius.SqLite.StoredProcedures;

namespace UsersTestApp.Procedures
{
    [StoredProcedure("sp_InsertUser", true)]
    [Parameter("FirstName", typeof(string))]
    [Parameter("LastName", typeof(string))]
    [Parameter("DateOfBirth", typeof(DateTime),ParameterDirection.Input, false)]
    [Parameter("UserID", typeof(long), ParameterDirection.ReturnValue)]
    public class InsertUser : IAdoProcedure
    {
        private readonly IAdoQueryRunner _queryRunner;
        private readonly IDataReaderFactory _dataReaderFactory;
        private readonly IParameterAccessor _parameterAccessor;

        private readonly IQuery _insertSql;
        private readonly IQuery _selectSql;

        public InsertUser(
            IAdoQueryRunner queryRunner, 
            IDataReaderFactory dataReaderFactory, 
            IParameterAccessor parameterAccessor, 
            IQueryBuilder queryBuilder)
        {
            _queryRunner = queryRunner;
            _dataReaderFactory = dataReaderFactory;
            _parameterAccessor = parameterAccessor;

            _insertSql = queryBuilder
                .InsertInto("tb_users", "FirstName", "LastName")
                .Values("@FirstName", "@LastName");

            _selectSql = queryBuilder
                .Select("UserID", "FirstName", "LastName")
                .From("tb_Users")
                .Where("FirstName = @FirstName")
                .And("LastName = @LastName");
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            _queryRunner.ExecuteNonQuery(context, _insertSql);
            _parameterAccessor.Set(context.Parameters, "UserID", context.Connection.LastInsertRowId);

            var sqLiteReader = _queryRunner.ExecuteReader(context, _selectSql);
            return _dataReaderFactory.Create(sqLiteReader, context);
        }
    }
}
