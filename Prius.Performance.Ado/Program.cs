using System;
using System.IO;

namespace Prius.Performance.Ado
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
