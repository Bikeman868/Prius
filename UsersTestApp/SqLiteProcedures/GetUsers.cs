using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.Contracts.Interfaces.Commands;
using Prius.SqLite.Interfaces;
using Prius.SqLite.StoredProcedures;

namespace UsersTestApp.SqLiteProcedures
{
    [StoredProcedure("sp_GetUsers", true)]
    [StoredProcedureParameter("FirstName", typeof(string))]
    [StoredProcedureParameter("LastName", typeof(string))]
    [StoredProcedureParameter("UserID", typeof(long), ParameterDirection.ReturnValue, false)]
    public class GetUsers : IStoredProcedure
    {
        private readonly IQueryRunner _queryRunner;
        private readonly IDataReaderFactory _dataReaderFactory;

        public GetUsers(IQueryRunner queryRunner, IDataReaderFactory dataReaderFactory)
        {
            _queryRunner = queryRunner;
            _dataReaderFactory = dataReaderFactory;
        }

        public IDataReader Execute(IList<IParameter> parameters, SQLiteConnection connection, SQLiteTransaction transaction, TextWriter messageOutput)
        {
            _queryRunner.ExecuteNonQuery(connection, "INSERT INTO tb_Users (FirstName,LastName)VALUES(@FirstName,@LastName)", parameters);
            parameters[2].Value = connection.LastInsertRowId;

            var sqLiteReader = _queryRunner.ExecuteReader(connection, "SELECT UserID, FirstName, LastName FROM tb_Users WHERE FirstName=@FirstName AND LastName=@LastName", parameters);

            return _dataReaderFactory.Create(sqLiteReader);
        }
    }
}
