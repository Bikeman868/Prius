using System.Collections.Generic;

namespace UsersTestApp.DataAccess
{
    public interface IDataAccessLayer
    {
        User AddUser(string repository, string firstName, string lastName);
        bool DeleteUser(string repository, int userId);
        IList<User> GetUsers(string repository);
    }
}
