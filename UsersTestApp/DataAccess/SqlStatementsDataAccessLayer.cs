using System.Collections.Generic;
using System.Linq;
using Prius.Contracts.Interfaces;

namespace UsersTestApp.DataAccess
{
    public class SqlStatementsDataAccessLayer : IDataAccessLayer
    {
        private readonly IContextFactory _contextFactory;
        private readonly ICommandFactory _commandFactory;


        public SqlStatementsDataAccessLayer(
            IContextFactory contextFactory,
            ICommandFactory commandFactory)
        {
            _contextFactory = contextFactory;
            _commandFactory = commandFactory;
        }

        public User AddUser(string repository, string firstName, string lastName)
        {
            var sql = "INSERT INTO tb_Users (FirstName,LastName)VALUES(@FirstName,@LastName);";
            sql += "SELECT UserID,FirstName,LastName FROM tb_Users WHERE FirstName=@FirstName AND LastName=@LastName;";
            using (var command = _commandFactory.CreateSql(sql))
            {
                command.AddParameter("FirstName", firstName);
                command.AddParameter("LastName", lastName);
                using (var context = _contextFactory.Create(repository))
                {
                    using (var users = context.ExecuteEnumerable<User>(command))
                    {
                        return users.FirstOrDefault();
                    }
                }
            }
        }

        public bool DeleteUser(string repository, int userId)
        {
            using (var command = _commandFactory.CreateSql("DELETE FROM tb_Users WHERE UserID=@UserID"))
            {
                command.AddParameter("UserID", userId);
                using (var context = _contextFactory.Create(repository))
                {
                    var rowsAffected = context.ExecuteNonQuery(command);
                    return rowsAffected == 1;
                }
            }
        }

        public IList<User> GetUsers(string repository)
        {
            using (var command = _commandFactory.CreateSql("SELECT UserID,FirstName,LastName FROM tb_Users"))
            {
                using (var context = _contextFactory.Create(repository))
                {
                    using (var users = context.ExecuteEnumerable<User>(command))
                    {
                        return users.ToList();
                    }
                }
            }
        }
    }
}
