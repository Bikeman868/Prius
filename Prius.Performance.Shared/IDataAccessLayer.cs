using System;
using System.Collections.Generic;

namespace Prius.Performance.Shared
{
    public interface IDataAccessLayer
    {
        /// <summary>
        /// Retrieves one customer from the database
        /// </summary>
        /// <param name="customerId">The unique ID of the customer</param>
        /// <param name="preloadOrders">Pass true to have orders loaded with the customer details.
        /// Pass false to have orders lazily loaded on first access</param>
        /// <returns>The customer with this ID</returns>
        ICustomer GetCustomer(long customerId, bool preloadOrders);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selector">A lambda expression that filters and orders the customers</param>
        /// <param name="preloadOrders">Pass true to have order details loaded with the customer details.
        /// Pass false to have orders lazily loaded on first access</param>
        /// <returns>A list customers from the database</returns>
        IList<ICustomer> GetCustomers(Func<IEnumerable<ICustomer>, IEnumerable<ICustomer>> selector, bool preloadOrders);
    }
}
