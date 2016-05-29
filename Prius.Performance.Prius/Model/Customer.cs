using System;
using System.Collections.Generic;
using System.Linq;
using Prius.Contracts.Attributes;
using Prius.Contracts.Interfaces;
using Prius.Performance.Shared;

namespace Prius.Performance.Prius.Model
{
    public class Customer: ICustomer
    {
        private readonly ICommandFactory _commandFactory;
        private readonly IContextFactory _contextFactory;

        private IList<IOrder> _orders;

        public Customer(
            ICommandFactory commandFactory,
            IContextFactory contextFactory)
        {
            _commandFactory = commandFactory;
            _contextFactory = contextFactory;
        }

        [Mapping("CustomerID")]
        public long CustomerId { get; set; }

        [Mapping("GivenNames")]
        public string GivenNames { get; set; }

        [Mapping("FamilyName")]
        public string FamilyName { get; set; }

        [Mapping("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [Mapping("Email")]
        public string Email { get; set; }

        public IList<IOrder> Orders 
        {
            get 
            {
                if (_orders == null)
                {
                    using (var command = _commandFactory.CreateStoredProcedure("dbo.sp_GetCustomerOrders"))
                    {
                        command.AddParameter("CustomerID", CustomerId);
                        using (var context = _contextFactory.Create("PerformanceTest"))
                        {
                            using (var orders = context.ExecuteEnumerable<Order>(command))
                            {
                                _orders = orders.Cast<IOrder>().ToList();
                            }
                        }
                    }
                }
                return _orders;
            }
            set { _orders = value; }
        }
    }
}
