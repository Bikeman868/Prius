using System;
using System.Collections.Generic;
using System.Data.Entity;
using Prius.Performance.Shared;

namespace Prius.Performance.EntityFramework
{
    internal class DataAccessLayer : DbContext, IDataAccessLayer
    {
        public ICustomer GetCustomer(long customerId, bool preloadOrders)
        {
            // Database.SqlQuery<Customers>("dbo.sp_GetCustomer");
            throw new NotImplementedException();
        }

        public IList<ICustomer> GetCustomers(Func<IEnumerable<ICustomer>, IEnumerable<ICustomer>> selector, bool preloadOrders)
        {
            throw new NotImplementedException();
        }
    }
}
