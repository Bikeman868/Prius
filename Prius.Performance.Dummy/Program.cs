using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
