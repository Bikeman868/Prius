using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Prius.Contracts.Interfaces;
using Prius.Performance.Prius.Model;
using Prius.Performance.Shared;

namespace Prius.Performance.Prius
{
    public class DataAccessLayer : IDataAccessLayer
    {
        private readonly IContextFactory _contextFactory;
        private readonly ICommandFactory _commandFactory;
        private readonly IMapper _mapper;
        private readonly IDataEnumeratorFactory _dataEnumeratorFactory;


        public DataAccessLayer(
            IContextFactory contextFactory,
            ICommandFactory commandFactory,
            IMapper mapper,
            IDataEnumeratorFactory dataEnumeratorFactory)
        {
            _contextFactory = contextFactory;
            _commandFactory = commandFactory;
            _mapper = mapper;
            _dataEnumeratorFactory = dataEnumeratorFactory;
        }

        public ICustomer GetCustomer(long customerId, bool preloadOrders)
        {
            using (var command = _commandFactory.CreateStoredProcedure("dbo.sp_GetCustomer"))
            {
                command.AddParameter("CustomerID", customerId);
                command.AddParameter("IncludeOrders", preloadOrders);

                using (var context = _contextFactory.Create("PerformanceTest"))
                {
                    if (preloadOrders)
                    {
                        // In this case the stored procedure returns two result sets, this
                        // requires slightly more code but is more efficient than calling
                        // the database twice.
                        using (var reader = context.ExecuteReader(command))
                        {
                            if (reader.Read())
                            {
                                var customer = _mapper.Map<Customer>(reader);
                                if (reader.NextResult())
                                {
                                    using (var orderEnumerator = _dataEnumeratorFactory.Create<Order>(reader))
                                        customer.Orders = orderEnumerator.Cast<IOrder>().ToList();
                                }
                                return customer;
                            }
                        }
                    }
                    else
                    {
                        // In this case the stored procedure returns a single set of data
                        // and mapping it to the customer model is very straigtforward.
                        using (var customers = context.ExecuteEnumerable<Customer>(command))
                        {
                            return customers.FirstOrDefault();
                        }
                    }
                }
            }
            return null;
        }

        public IList<ICustomer> GetCustomers(Func<IEnumerable<ICustomer>, IEnumerable<ICustomer>> selector, bool preloadOrders)
        {
            if (preloadOrders)
                return GetCustomersAndOrders(selector);
             
            using (var context = _contextFactory.Create("PerformanceTest"))
            {
                using (var command = _commandFactory.CreateStoredProcedure("sp_GetAllCustomers"))
                {
                    using (var data = context.ExecuteEnumerable<Customer>(command))
                        return selector(data).ToList();
                }
            }
        }

        /// <summary>
        /// This implementation assumes that you are retrieving most of the customers, and therefore 
        /// most of the orders.
        /// Retrieves all customers and all orders in parallel then uses LINQ to find the orders for
        /// each customer. This means only hitting the DB twice (in parallel) and doing the join
        /// in C#. If you were typically retrieving only a few customers then you will be better off
        /// lazy loading the orders (passing preloadOrders = false) or filtering the customers first, 
        /// then only retrieving the orders for those customers using a different stored procedure.
        /// </summary>
        private IList<ICustomer> GetCustomersAndOrders(Func<IEnumerable<ICustomer>, IEnumerable<ICustomer>> selector)
        {
            var customersContext = _contextFactory.Create("PerformanceTest");
            var customersCommand = _commandFactory.CreateStoredProcedure("dbo.sp_GetAllCustomers");

            var ordersContext = _contextFactory.Create("PerformanceTest");
            var ordersCommand = _commandFactory.CreateStoredProcedure("dbo.sp_GetAllOrders");

            try
            {
                var customersResult = customersContext.BeginExecuteEnumerable(customersCommand);
                var ordersResult = ordersContext.BeginExecuteEnumerable(ordersCommand);
                WaitHandle.WaitAll(new[] {customersResult.AsyncWaitHandle, ordersResult.AsyncWaitHandle});

                List<Order> orderList;
                using (var orderRecords = ordersContext.EndExecuteEnumerable<Order>(ordersResult))
                    orderList = orderRecords.ToList();

                using (var customerRecords = customersContext.EndExecuteEnumerable<Customer>(customersResult))
                {
                    // Note that this version populates the orders prior to selection so that
                    // the selector function can select based on the order information. If you don't
                    // need this, then it would be more efficient to populate the orders only for
                    // the customers that were selected.
                    var customerList = customerRecords
                        .Select(c =>
                        {
                            c.Orders = orderList
                                .Where(o => o.CustomerId == c.CustomerId)
                                .Cast<IOrder>()
                                .ToList();
                            return c;
                        });

                    return selector(customerList).ToList();
                }
            }
            finally
            {
                customersContext.Dispose();
                customersCommand.Dispose();
                ordersContext.Dispose();
                ordersCommand.Dispose();
            }
        }
    }
}
