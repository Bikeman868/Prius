using System;
using System.IO;
using System.Text;

namespace Prius.Performance.Dummy
{
    class Program
    {
        static void Main(string[] args)
        {
            var dal = new DataAccessLayer();
            var test = new Shared.PerformanceTest(dal);

            using (var output = Console.OpenStandardOutput())
            {
                using (var writer = new StreamWriter(output))
                {
                    test.RunTests(writer);
                }
            }
            Console.ReadKey();

            #region This code was used to generate the SQL script that creates the test database

            var allCustomers = dal.GetCustomers(c => c, true);

            var sql = new StringBuilder();
            foreach (var customer in allCustomers)
            {
                sql.AppendFormat("INSERT INTO [Customers] (FamilyName,GivenNames,DateOfBirth,Email) VALUES('{0}','{1}','{2:yyyyMMdd}','{3}')\n", 
                    customer.FamilyName, customer.GivenNames, customer.DateOfBirth, customer.Email);

                foreach (var order in customer.Orders)
                {
                    sql.AppendFormat(
                        "INSERT INTO [Orders] (CustomerID,OrderDate,OrderTotal,TaxAmount,ShippingAmount) VALUES({0},'{1:yyyyMMdd}',{2},{3},{4})\n",
                        customer.CustomerId, order.OrderDate, order.OrderTotal, order.TaxAmount, order.ShippingAmount);
                }
            }

            var sqlText = sql.ToString();

            #endregion
        }
    }
}
