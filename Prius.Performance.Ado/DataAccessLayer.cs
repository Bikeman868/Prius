using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Prius.Performance.Ado.Model;
using Prius.Performance.Shared;
using System.Configuration;

namespace Prius.Performance.Ado
{
    public class DataAccessLayer: IDataAccessLayer
    {
        public ICustomer GetCustomer(long customerId, bool preloadOrders)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["PriusPerformanceTests"].ConnectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "dbo.sp_GetCustomer";

                    var customerIdParameter = new SqlParameter("@CustomerID", customerId);
                    command.Parameters.Add(customerIdParameter);

                    var includeOrdersParameter = new SqlParameter("@IncludeOrders", SqlDbType.Bit) {Value = 0};
                    command.Parameters.Add(includeOrdersParameter);

                    connection.Open();

                    var reader = command.ExecuteReader();
                    try
                    {
                        if (!reader.Read()) return null;

                        var customer = new Customer
                        {
                            CustomerId = Convert.ToInt64(reader["CustomerId"]),
                            GivenNames = reader["GivenNames"].ToString(),
                            FamilyName = reader["FamilyName"].ToString(),
                            DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                            Email = reader["Email"].ToString()
                        };

                        if (preloadOrders)
                            customer.Orders = GetCustomerOrders(customerId);

                        return customer;
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }

        public IList<ICustomer> GetCustomers(Func<IEnumerable<ICustomer>, IEnumerable<ICustomer>> selector, bool preloadOrders)
        {
            var customers = new List<Customer>();

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["PriusPerformanceTests"].ConnectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "dbo.sp_GetAllCustomers";

                    connection.Open();

                    var reader = command.ExecuteReader();
                    try
                    {
                        while (reader.Read())
                        {
                            var customer = new Customer
                            {
                                CustomerId = Convert.ToInt64(reader["CustomerId"]),
                                GivenNames = reader["GivenNames"].ToString(),
                                FamilyName = reader["FamilyName"].ToString(),
                                DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                                Email = reader["Email"].ToString()
                            };

                            customers.Add(customer);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }

            var filteredCustomers = selector(customers).ToList();

            if (!preloadOrders)
                return filteredCustomers;

            foreach (var customer in filteredCustomers.Cast<Customer>())
                customer.Orders = GetCustomerOrders(customer.CustomerId);

            return filteredCustomers;
        }

        public IList<IOrder> GetCustomerOrders(long customerId)
        {
            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["PriusPerformanceTests"].ConnectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "dbo.sp_GetCustomerOrders";

                    var customerIdParameter = new SqlParameter("@CustomerID", customerId);
                    command.Parameters.Add(customerIdParameter);

                    connection.Open();

                    var reader = command.ExecuteReader();
                    try
                    {
                        var orders = new List<IOrder>();
                        while (reader.Read())
                        {
                            var order = new Order
                            {
                                OrderId = Convert.ToInt64(reader["OrderId"]),
                                CustomerId = Convert.ToInt64(reader["CustomerId"]),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                OrderTotal = Convert.ToInt64(reader["OrderTotal"]),
                                TaxAmount = Convert.ToInt64(reader["TaxAmount"]),
                                ShippingAmount = Convert.ToInt64(reader["ShippingAmount"])
                            };
                            if (!reader.IsDBNull(reader.GetOrdinal("ShippedDate")))
                                order.ShippedDate = reader.GetSqlDateTime(reader.GetOrdinal("ShippedDate")).Value;
                            order.InvoiceTotal = order.OrderTotal + order.TaxAmount + order.ShippingAmount;

                            orders.Add(order);
                        }
                        return orders;
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
        }
    }
}
