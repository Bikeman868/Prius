using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prius.Performance.Shared
{
    public class PerformanceTest
    {
        private readonly IDataAccessLayer _dataAccessLayer;

        public PerformanceTest(IDataAccessLayer dataAccessLayer)
        {
            _dataAccessLayer = dataAccessLayer;
        }

        public void RunTests(TextWriter resultWriter)
        {
            RunTest("do nothing", resultWriter, new[]{1, 1000, 10000}, () => { });

            RunTest("get one customer", resultWriter, new[]{1, 10, 100}, () => _dataAccessLayer.GetCustomer(1, false));

            RunTest("get one customer with orders", resultWriter, new[]{1, 10, 100}, () => _dataAccessLayer.GetCustomer(1, true));

            RunTest("get one customer and lazy load orders", resultWriter, new[]{1, 10, 100}, () =>
            {
                var customer = _dataAccessLayer.GetCustomer(1, false);
                var orderCount = customer.Orders.Count;
            });

            RunTest("get select customers", resultWriter, new[] { 1, 100, 1000 }, () => 
            {
                var customerList = _dataAccessLayer.GetCustomers(
                    customers => customers
                        .Where(c => c.FamilyName.StartsWith("A"))
                        .OrderByDescending(c => c.DateOfBirth)
                        .Take(10), 
                    false);
            });

            RunTest("get select customers with orders", resultWriter, new[] { 1, 100, 1000 }, () =>
            {
                var customerList = _dataAccessLayer.GetCustomers(
                    customers => customers
                        .Where(c =>
                        {
                            var age = c.CalculateAge();
                            return age >= 18 && age <= 65;
                        })
                        .OrderBy(c => c.FamilyName),
                    true);
            });

            RunTest("get all customers", resultWriter, new[]{ 1, 100}, () =>
            {
                _dataAccessLayer.GetCustomers(customers => customers, false);
            });

            RunTest("get all customers with orders", resultWriter, new[] { 1, 100 }, () =>
            {
                _dataAccessLayer.GetCustomers(customers => customers, true);
            });
        }

        private void RunTest(string name, TextWriter resultWriter, IEnumerable<int> repeatCounts, Action test)
        {
            foreach (var repeatCount in repeatCounts)
            {
                var startTicks = HighPrecisionTimer.Ticks;

                for (var i = 0; i < repeatCount; i++)
                    test();

                var endTicks = HighPrecisionTimer.Ticks;

                var elapsedMicroseconds = HighPrecisionTimer.ElapsedMicroseconds(startTicks, endTicks);
                var elapsedMilliseconds = HighPrecisionTimer.ElapsedMilliseconds(startTicks, endTicks);

                if (elapsedMilliseconds < 1)
                {
                    resultWriter.WriteLine(
                        "{0} iterations of {1} completed in {2:g3}us. Average of {3:g3}us per iteration",
                        repeatCount, name, elapsedMicroseconds, elapsedMicroseconds/repeatCount);
                }
                else
                {
                    resultWriter.WriteLine(
                        "{0} iterations of {1} completed in {2:g3}ms. Average of {3:g3}ms per iteration",
                        repeatCount, name, elapsedMilliseconds, elapsedMilliseconds / repeatCount);
                }
            }
        }
    }
}
