﻿using System;
using System.Collections.Generic;
using System.Linq;
using Prius.Contracts.Interfaces;

namespace UsersTestApp.DataAccess
{
    public class StoredProcedureDataAccessLayer : IDataAccessLayer
    {
        private readonly IContextFactory _contextFactory;
        private readonly ICommandFactory _commandFactory;


        public StoredProcedureDataAccessLayer(
            IContextFactory contextFactory,
            ICommandFactory commandFactory)
        {
            _contextFactory = contextFactory;
            _commandFactory = commandFactory;
        }

        public User AddUser(string repository, string firstName, string lastName, DateTime dateOfBirth)
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_InsertUser"))
            {
                command.AddParameter("FirstName", firstName);
                command.AddParameter("LastName", lastName);
                command.AddParameter("DateOfBirth", dateOfBirth);
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
            using (var command = _commandFactory.CreateStoredProcedure("sp_DeleteUser"))
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
            using (var command = _commandFactory.CreateStoredProcedure("sp_GetUsers"))
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

        public void TimeoutTest(string repository)
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_TimeoutTest"))
            {
                using (var context = _contextFactory.Create(repository))
                {
                    context.ExecuteNonQuery(command);
                }
            }
        }

        public void ExceptionTest(string repository)
        {
            using (var command = _commandFactory.CreateStoredProcedure("sp_ExceptionTest"))
            {
                using (var context = _contextFactory.Create(repository))
                {
                    context.ExecuteNonQuery(command);
                }
            }
        }
    }
}
