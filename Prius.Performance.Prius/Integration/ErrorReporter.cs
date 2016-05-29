using System;
using System.Data.SqlClient;
using Prius.Contracts.Interfaces;

namespace Prius.Performance.Prius
{
    public class ErrorReporter : IErrorReporter
    {
        public void ReportError(Exception e, string subject, params object[] otherInfo)
        {
            if (e == null)
                Console.WriteLine("Warning: " + subject);
            else
                Console.WriteLine("Exception: " + e.Message);
        }

        public void ReportError(Exception e, SqlCommand cmd, string subject, params object[] otherInfo)
        {
            if (e == null)
                Console.WriteLine("DB warning: " + subject + " executing " + cmd.CommandText);
            else
                Console.WriteLine("DB exception: " + e.Message + " executing " + cmd.CommandText);
        }

        public void Dispose()
        {
        }
    }
}
