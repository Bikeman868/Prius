using System;
using System.Collections.Generic;
using System.Linq;
using Prius.Performance.Shared;

namespace Prius.Performance.Dummy
{
    public class DataAccessLayer : IDataAccessLayer
    {
        private readonly Random _random = new Random();
        private long _nextOrderId = 1;

        public ICustomer GetCustomer(long customerId, bool preloadOrders)
        {
            var customer = new Customer(customerId, LoadCustomerOrders)
            {
                CustomerId = customerId,
                DateOfBirth = DateTime.UtcNow.AddDays(_random.NextDouble() * -30000 - 5000),
                GivenNames = RandomGivenNames(),
                FamilyName = RandomFamilyName(),
                Email = "customer" + customerId + "@mailinator.com"
            };

            if (preloadOrders)
                customer.Orders = LoadCustomerOrders(customerId).ToList();

            return customer;
        }

        public IList<ICustomer> GetCustomers(Func<IEnumerable<ICustomer>, IEnumerable<ICustomer>> selector, bool preloadOrders)
        {
            var customers = Enumerable
                .Range(1, 1000)
                .Select(id => GetCustomer(id, preloadOrders));
            return selector(customers).ToList();
        }

        private IEnumerable<IOrder> LoadCustomerOrders(long customerId)
        {
            var count = _random.Next(10);
            var orders = new List<IOrder>(count);
            for (var i = 0; i < count; i++)
            {
                var order = new Order()
                {
                    CustomerId = customerId,
                    OrderId = _nextOrderId++,
                    OrderDate = DateTime.UtcNow.AddDays(_random.NextDouble() * -30)
                };
                orders.Add(order);
            }
            return orders;
        }

        private string RandomGivenNames()
        {
            var boysNames = new[] { "Muhammad", "Oliver", "Jack", "Noah", "Jacob", "Charlie", "Harry", "Joshua", "James", "Ethan", "Thomas", "William", "Henry", "Oscar", "Daniel", "Sam", "Luca", "Alden", "Arthur", "Aaron", "Michael", "David", "Tyler", "Ben"};
            var girlsNames = new[] {"Olivia", "Emily", "Sophia", "Lilly", "Isabella", "Amelia", "Sophie", "Ava", "Chloe", "Poppy", "Jessica", "Mia", "Ella", "Grace", "Lucy", "Alice", "Lola", "Evelyn", "Esme", "Georgia", "Rose", "Amber", "Eliza", "Harriet", "Jasmine"};
            var names = _random.Next(2) == 1 ? boysNames : girlsNames;
            var count = _random.Next(3) + 1;

            var givenNames = string.Empty;
            for (var i = 0; i < count; i++)
            {
                if (i > 0) givenNames += " ";
                givenNames += names[_random.Next(names.Length)];
            }

            return givenNames;
        }

        private string RandomFamilyName()
        {
            var familyNames = new[] {"Smith","Jones","Taylor","Williams","Brown", "Davies", "Evans", "Wilson", "Thomas", "Roberts", "Johnson", "Lewis", "Walker", "Robinson", "Wood"};
            return familyNames[_random.Next(familyNames.Length)];
        }
    }
}
