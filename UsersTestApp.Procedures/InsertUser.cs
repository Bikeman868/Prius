﻿using System;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.SQLite.Interfaces;
using Prius.SQLite.Procedures;
using Prius.SQLite.QueryBuilder;

namespace UsersTestApp.Procedures
{
    [Procedure("sp_InsertUser")]
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
                .InsertInto("tb_users", "FirstName", "LastName", "DateOfBirth")
                .Values("@FirstName", "@LastName", "@DateOfBirth");

            _selectSql = queryBuilder
                .Select("UserID", "FirstName", "LastName", "DateOfBirth")
                .From("tb_Users")
                .Where("FirstName = @FirstName")
                .And("LastName = @LastName");
        }

        public IDataReader Execute(AdoExecutionContext context)
        {
            _queryRunner.ExecuteNonQuery(context, _insertSql);
            _parameterAccessor.Return(context.Parameters, context.Connection.LastInsertRowId);

            var sqLiteReader = _queryRunner.ExecuteReader(context, _selectSql);
            return _dataReaderFactory.Create(sqLiteReader, context);
        }
    }
}
