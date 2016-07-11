using System;
using System.Collections.Generic;
using Prius.Performance.Shared;

namespace Prius.Performance.EntityFramework.Model
{
    public class Customer: ICustomer
    {
        public long CustomerId { get; set; }
        public string GivenNames { get; set; }
        public string FamilyName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }

        public IList<IOrder> Orders
        {
            get { throw new NotImplementedException(); }
        }
    }
}
