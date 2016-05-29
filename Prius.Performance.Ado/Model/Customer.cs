using System;
using System.Collections.Generic;
using Prius.Performance.Shared;

namespace Prius.Performance.Ado.Model
{
    public class Customer: ICustomer
    {
        private IList<IOrder> _orders;

        public long CustomerId { get; set; }
        public string GivenNames { get; set; }
        public string FamilyName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }

        public IList<IOrder> Orders 
        {
            get 
            {
                if (_orders == null)
                {
                    var dataAccessLayer = new DataAccessLayer();
                    _orders = dataAccessLayer.GetCustomerOrders(CustomerId);
                }
                return _orders;
            }
            set { _orders = value; }
        }
    }
}
