using System;
using System.Data;
using Prius.Contracts.Attributes;
using Prius.SqLite.Schema;

namespace UsersTestApp.DataAccess
{
    [SchemaTable("tb_Users")]
    public class User
    {
        [Mapping("userID", null)]
        [SchemaColumn("userID", DbType.UInt32, ColumnAttributes.UniqueKey)]
        public int? UserId { get; set; }

        [Mapping("firstName", null)]
        [SchemaColumn("firstName", DbType.String, ColumnAttributes.NotNull)]
        [SchemaIndex("ix_FullName", IndexAttributes.Unique)]
        public string FirstName { get; set; }

        [Mapping("lastName", null)]
        [SchemaColumn("lastName", DbType.String, ColumnAttributes.NotNull)]
        [SchemaIndex("ix_FullName", IndexAttributes.Unique)]
        public string LastName { get; set; }

        [Mapping("dateOfBirth", null)]
        [SchemaColumn("dateOfBirth", DbType.DateTime)]
        public DateTime DateOfBirth { get; set; }
    }
}
