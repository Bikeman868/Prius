using System;
using System.Collections.Generic;
using System.Linq;
using Prius.Performance.Shared;

namespace Prius.Performance.Dummy
{
    public class Customer: ICustomer
    {
        private readonly Func<long, IEnumerable<IOrder>>  _getOrders;
        private IList<IOrder> _orders;

        public Customer(long id, Func<long, IEnumerable<IOrder>> getOrders)
        {
            CustomerId = id;
            _getOrders = getOrders;
        }

        public long CustomerId { get; set; }
        public string GivenNames { get; set; }
        public string FamilyName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }

        public IList<IOrder> Orders
        {
            get { return _orders ?? (_orders = _getOrders(CustomerId).ToList()); }
            set { _orders = value; }
        }
    }
}
